using System;
using System.Linq;
using CommandCentral.Authentication;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using Random = CommandCentral.Utilities.Random;

namespace CommandCentral.Controllers.AccountManagementControllers
{
    /// <summary>
    /// Authentication is the means by which a client identifies him or herself and obtains a session id.  
    /// A session id is expected to be passed in every subsequent request in order to identify a user's current session.
    /// </summary>
    public class AuthenticationController : CommandCentralController
    {
        /// <summary>
        /// Authenticates a user given a username and password.  Returns a header, X-Session-Id, which should be sent back to the API on every request to identify your client.
        /// </summary>
        /// <param name="dto">A dto identifying the information needed to login a user.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(204)]
        public IActionResult Login([FromBody] DTOs.Authentication.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var person = DBSession.Query<Person>().SingleOrDefault(x => x.Username == dto.Username);

            if (person == null)
                return Unauthorized();

            if (!PasswordHash.ValidatePassword(dto.Password, person.PasswordHash))
            {
                Events.EventManager.OnLoginFailed(new Events.Args.LoginFailedEventArgs
                {
                    Person = person
                }, this);

                //Now we also need to add the event to client's account history.
                person.AccountHistory.Add(new AccountHistoryEvent
                {
                    AccountHistoryEventType = AccountHistoryTypes.FailedLogin,
                    EventTime = CallTime,
                    Id = Guid.NewGuid(),
                    Person = person
                });

                CommitChanges();

                return Unauthorized();
            }

            //The client is who they claim to be so let's make them an authentication session.
            var authSession = new AuthenticationSession
            {
                Id = Guid.NewGuid(),
                Token = Random.CreateCryptographicallySecureGuid(),
                IsActive = true,
                LastUsedTime = CallTime,
                LoginTime = CallTime,
                Person = person
            };

            //Also put the account history on the client.
            person.AccountHistory.Add(new AccountHistoryEvent
            {
                AccountHistoryEventType = AccountHistoryTypes.Login,
                EventTime = CallTime,
                Id = Guid.NewGuid(),
                Person = person
            });

            Response.Headers.Add("Access-Control-Expose-Headers", "X-Session-Id");
            Response.Headers["X-Session-Id"] =
                new Microsoft.Extensions.Primitives.StringValues(authSession.Token.ToString());

            DBSession.Save(authSession);

            CommitChanges();

            return NoContent();
        }

        /// <summary>
        /// Logs out the user and invalidates the session identified by the X-Session-Id header.  Client muster be the owner of the session.
        /// </summary>
        /// <param name="sessionId">The session id to log out.</param>
        /// <returns></returns>
        [HttpDelete]
        [RequireAuthentication]
        [ProducesResponseType(204)]
        public IActionResult Logout([FromHeader(Name = "X-Session-Id")] string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return BadRequest();

            var authSession = DBSession.Get<AuthenticationSession>(Guid.Parse(sessionId));

            if (authSession == null)
                return BadRequest();

            if (authSession.Person != User)
                return Forbid();

            authSession.IsActive = false;
            authSession.LogoutTime = CallTime;

            CommitChanges();

            return NoContent();
        }
    }
}