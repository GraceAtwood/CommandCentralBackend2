using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Cfg.XmlHbmBinding;
using NHibernate.Criterion;
using NHibernate.Linq;
using Random = CommandCentral.Utilities.Random;

namespace CommandCentral.Controllers.AccountManagementControllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class PasswordResetController : CommandCentralController
    {
        /// <summary>
        /// Allows only clients with admin tools to query the password resets.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="timeSubmitted"></param>
        /// <returns></returns>
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.PasswordReset.Get>))]
        public IActionResult Get([FromQuery] string person, [FromQuery] DTOs.DateTimeRangeQuery timeSubmitted)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var predicate = ((Expression<Func<PasswordReset, bool>>) null)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddDateTimeQueryExpression(x => x.TimeSubmitted, timeSubmitted);

            var results = DBSession.Query<PasswordReset>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToList()
                .Select(x => new DTOs.PasswordReset.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Allows only clients with admin tools to get password resets by id.
        /// </summary>
        /// <param name="id">The reset object's id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.PasswordReset.Get))]
        public IActionResult Get(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var confirmation = DBSession.Get<PasswordReset>(id);
            if (confirmation == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.PasswordReset.Get(confirmation));
        }

        [HttpPost("/start")]
        [ProducesResponseType(204)]
        public IActionResult Post([FromBody] DTOs.PasswordReset.PostStart dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (String.IsNullOrWhiteSpace(dto.ContinueLink))
                return BadRequest($"The parameter '{nameof(dto.ContinueLink)} must not be empty.");

            var email = DBSession.Query<EmailAddress>()
                .SingleOrDefault(x => x.Address == dto.Email);

            if (email == null)
                return NotFoundParameter(dto.Email, nameof(dto.Email));

            if (email.Person.SSN != dto.SSN)
                return BadRequest("This SSN and email combination does not match.");
                
            if (!email.Person.IsClaimed)
                return BadRequest("This profile has not been claimed. Please register to set your passowrd.");
            
            var reset = new PasswordReset
            {
                Id = Guid.NewGuid(),
                Person = email.Person,
                TimeSubmitted = CallTime,
                ResetToken = Random.CreateCryptographicallySecureGuid()
            };

            var redirectUrl = dto.ContinueLink.Replace($"[{nameof(PasswordReset.ResetToken)}]",
                reset.ResetToken.ToString());

            if (Uri.IsWellFormedUriString(redirectUrl, UriKind.RelativeOrAbsolute))
                return BadRequest($"The given value of '{nameof(dto.ContinueLink)}' plus the confirmation token was " +
                                  $"not a valid URI.");

            var results = reset.Validate();

            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            var existingReset = DBSession.Query<PasswordReset>()
                .SingleOrDefault(x => x.Person == reset.Person);
            
            reset.Person.AccountHistory.Add(new AccountHistoryEvent
            {
                AccountHistoryEventType = AccountHistoryTypes.PasswordResetStarted,
                EventTime = CallTime,
                Id = Guid.NewGuid(),
                Person = reset.Person
            });

            using (var transaction = DBSession.BeginTransaction())
            {
                if (existingReset != null)
                    DBSession.Delete(existingReset);

                DBSession.Save(reset);
                
                DBSession.Update(reset.Person);
                transaction.Commit();
            }
            
            //TODO: Send email to client with reset complete link

            return NoContent();

        }
    }
}
