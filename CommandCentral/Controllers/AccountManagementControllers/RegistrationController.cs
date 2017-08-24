using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using CommandCentral.Authentication;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using Random = CommandCentral.Utilities.Random;

namespace CommandCentral.Controllers.AccountManagementControllers
{
    /// <summary>
    /// The controller for all registration actions
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class RegistrationController : CommandCentralController
    {
        /// <summary>
        /// Queries the account registrations.  Client must have access to admin tools.
        /// </summary>
        /// <param name="isCompleted">A boolean query for whether or not account registrations have been completed.</param>
        /// <param name="person">The person for whom the account registration was made.</param>
        /// <param name="timeSubmitted">The time the account registration was submitted.</param>
        /// <param name="timeCompleted">The time the account registration was completed.</param>
        /// <returns></returns>
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Registration.Get>))]
        public IActionResult Get([FromQuery] bool? isCompleted, [FromQuery] string person, 
            [FromQuery] DTOs.DateTimeRangeQuery timeSubmitted, [FromQuery] DTOs.DateTimeRangeQuery timeCompleted)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();
            
            var predicate = ((Expression<Func<AccountRegistration, bool>>) null)
                .AddNullableBoolQueryExpression(x => x.IsCompleted, isCompleted)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddDateTimeQueryExpression(x => x.TimeSubmitted, timeSubmitted)
                .AddDateTimeQueryExpression(x => x.TimeCompleted, timeCompleted);

            var results = DBSession.Query<AccountRegistration>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToList()
                .Select(x => new DTOs.Registration.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Retrieves an account registration with the given id.
        /// </summary>
        /// <param name="id">The if of the acccount registration to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.Registration.Get))]
        public IActionResult Get(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();
            
            var confirmation = DBSession.Get<AccountRegistration>(id);
            if (confirmation == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.Registration.Get(confirmation));
        }
        
        /// <summary>
        /// Starts the account registration process by creating an account registration and then sending an email to the client containing the registration token.  
        /// The registration token, a Guid, will be inserted into your given continue link by replacing the text '[RegistrationToken]' with the actual token.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("start")]
        [ProducesResponseType(204)]
        public IActionResult Post([FromBody] DTOs.Registration.PostStart dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var person = DBSession.Query<Person>()
                .SingleOrDefault(x => x.SSN == dto.SSN);

            if (person == null)
                return NotFoundParameter(dto.SSN, nameof(dto.SSN));

            if (person.IsClaimed)
                return Forbid();

            var milEmailAddress = person.EmailAddresses.Where(x => x.IsDoDEmailAddress());
            if (!milEmailAddress.Any())
                return BadRequest($"The profile identified by the SSN '{dto.SSN}' has no associated .mil email " +
                                  "addresses.  In order to start registration, you must have at least one.");

            if (String.IsNullOrWhiteSpace(dto.ContinueLink))
                return BadRequest($"The parameter '{nameof(dto.ContinueLink)}' must not be empty.");
            
            var registration = new AccountRegistration
            {
                RegistrationToken = Random.CreateCryptographicallySecureGuid(),
                Id = Guid.NewGuid(),
                Person = person,
                TimeSubmitted = CallTime
            };

            var finalRedirectURL = dto.ContinueLink.Replace($"[{nameof(AccountRegistration.RegistrationToken)}]",
                registration.RegistrationToken.ToString());
            
            if (!Uri.IsWellFormedUriString(finalRedirectURL, UriKind.RelativeOrAbsolute))
                return BadRequest($"The given value of '{nameof(dto.ContinueLink)}' plus the confirmation token" +
                                  " was not a valid URI.");

            var results = registration.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            var existingRegistration = DBSession.Query<AccountRegistration>()
                .SingleOrDefault(x => x.Person == registration.Person);
            
            if (existingRegistration != null && existingRegistration.IsCompleted)
                throw new Exception("An existing registration that was completed was found for a person " +
                                    "who did not have .IsClaimed set to true.");

            registration.Person.AccountHistory.Add(new AccountHistoryEvent
            {
                AccountHistoryEventType = AccountHistoryTypes.RegistrationStarted,
                EventTime = CallTime,
                Id = Guid.NewGuid(),
                Person = registration.Person
            });
            
            using (var transaction = DBSession.BeginTransaction())
            {
                if (existingRegistration != null)
                    DBSession.Delete(existingRegistration);
                
                DBSession.Save(registration);
                
                DBSession.Update(registration.Person);
                transaction.Commit();
            }
            
            //TODO: Send email to client with details.
            
            return NoContent();
        }

        /// <summary>
        /// Completes the registration process
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("complete")]
        [ProducesResponseType(204)]
        public IActionResult Post([FromBody] DTOs.Registration.PostComplete dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var registration = DBSession.Query<AccountRegistration>()
                .SingleOrDefault(x => x.RegistrationToken == dto.RegistrationToken);
            
            if (registration == null)
                return NotFoundParameter(dto.RegistrationToken, nameof(dto.RegistrationToken));

            if (registration.IsCompleted || registration.IsAgedOff())
                return Forbid();
            
            if (registration.Person.IsClaimed)
                throw new Exception("An existing registration that was completed was found for a person " +
                                    "who did not have .IsClaimed set to true.");

            if (String.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 10)
                return BadRequest("Password must be at least 10 characters.");

            if (DBSession.Query<Person>().Any(x => x.Username == dto.Username))
                return Conflict("That username is already taken.");
            
            registration.Person.Username = dto.Username;
            registration.Person.PasswordHash = PasswordHash.CreateHash(dto.Password);
            registration.Person.IsClaimed = true;

            var personValidationResult = registration.Person.Validate();
            if (!personValidationResult.IsValid)
                return BadRequest(personValidationResult.Errors.Select(x => x.ErrorMessage));

            registration.Person.AccountHistory.Add(new AccountHistoryEvent
            {
                AccountHistoryEventType = AccountHistoryTypes.RegistrationCompleted,
                EventTime = CallTime,
                Id = Guid.NewGuid(),
                Person = registration.Person
            });

            registration.IsCompleted = true;
            registration.TimeCompleted = CallTime;

            var registrationValidationResult = registration.Validate();
            if (!registrationValidationResult.IsValid)
                return BadRequest(registrationValidationResult.Errors.Select(x => x.ErrorMessage));
            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Update(registration);
                DBSession.Update(registration.Person);
                transaction.Commit();
            }

            return NoContent();
        }
    }
}