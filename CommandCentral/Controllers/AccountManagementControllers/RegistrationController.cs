using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authentication;
using CommandCentral.Authorization;
using CommandCentral.Email;
using CommandCentral.Email.Models;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Events.Args;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using Random = CommandCentral.Utilities.Random;

namespace CommandCentral.Controllers.AccountManagementControllers
{
    /// <summary>
    /// Registration is the process by which clients can claim an account.  
    /// The client should send a request to start registration to the /start endpoint along with the client's SSN.  
    /// If registration hasn't already been started for the profile with that SSN, an email will be sent to the client.  
    /// It's important to note that the profile must have a .mil email address or else registration will fail.  
    /// The email will contain a link which is built by replacing the text '[RegistrationToken]' in your continue link with the actual token.
    /// When the client clicks that link, you should submit the given registration token along with a username and password for your client to the /complete endpoint.
    /// </summary>
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

            var finalRedirectURL = dto.ContinueLink.Replace("[RegistrationToken]",
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

            if (existingRegistration != null)
            {
                DBSession.Delete(existingRegistration);
                CommitChanges();
            }
            
            DBSession.Save(registration);

            CommitChanges();

            var message = new CCEmailMessage()
                .Subject("Registration Started")
                .HighPriority();

            var sendToAddress = person.EmailAddresses.FirstOrDefault();
            if (sendToAddress != null)
            {
                message.To(sendToAddress)
                    .BodyFromTemplate(Templates.RegistrationStartedTemplate,
                        new RegistrationStarted(person, finalRedirectURL))
                    .Send();
            }

            return NoContent();
        }

        /// <summary>
        /// Completes the registration process.
        /// </summary>
        /// <param name="dto">A dto containing the information needed to complete the registration process.</param>
        /// <returns></returns>
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

            CommitChanges();
            
            Events.EventManager.OnAccountRegistered(new AccountRegistrationEventArgs(registration), this);

            return NoContent();
        }
    }
}