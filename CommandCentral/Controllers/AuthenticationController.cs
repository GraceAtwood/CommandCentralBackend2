using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Authentication;

namespace CommandCentral.Controllers
{
    /// <summary>
    /// Authentication is the means by which a client identifies him or herself and obtains a session id.  
    /// A session id is expected to be passed in every subsequent request in order to identify a user's current session.
    /// </summary>
    [Route("api/[controller]")]
    public class AuthenticationController : CommandCentralController
    {
        [HttpPost]
        public IActionResult Login([FromBody] DTOs.Authentication.Post dto)
        {
            using (var transaction = DBSession.BeginTransaction())
            {
                var person = DBSession.QueryOver<Person>().Where(x => x.Username == dto.Username).SingleOrDefault();

                if (person == null)
                    return Unauthorized();

                if (!PasswordHash.ValidatePassword(dto.Password, person.PasswordHash))
                {

                    Events.EventManager.OnLoginFailed(new Events.Args.LoginFailedEventArgs
                    {
                        Person = person
                    });

                    //Now we also need to add the event to client's account history.
                    person.AccountHistory.Add(new AccountHistoryEvent
                    {
                        AccountHistoryEventType = ReferenceListHelper<AccountHistoryType>.Find("Failed Login"),
                        EventTime = this.CallTime,
                        Id = Guid.NewGuid(),
                        Person = person
                    });

                    DBSession.Update(person);

                    transaction.Commit();
                    return Unauthorized();
                }

                //The client is who they claim to be so let's make them an authentication session.
                AuthenticationSession ses = new AuthenticationSession
                {
                    Id = Guid.NewGuid(),
                    IsActive = true,
                    LastUsedTime = CallTime,
                    LoginTime = CallTime,
                    Person = person
                };

                //Now insert it
                DBSession.Save(ses);

                //Also put the account history on the client.
                person.AccountHistory.Add(new AccountHistoryEvent
                {
                    AccountHistoryEventType = ReferenceListHelper<AccountHistoryType>.Find("Login"),
                    EventTime = CallTime,
                    Id = Guid.NewGuid(),
                    Person = person
                });

                Response.Headers.Add("Access-Control-Expose-Headers", "X-Session-Id");
                Response.Headers["X-Session-Id"] = new Microsoft.Extensions.Primitives.StringValues(ses.Id.ToString());

                transaction.Commit();

                return NoContent();
            }
        }

        [HttpDelete]
        public IActionResult Logout([FromHeader] string sessionId)
        {
            using (var transaction = DBSession.BeginTransaction())
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                    return BadRequest();

                var authSession = DBSession.Get<AuthenticationSession>(Guid.Parse(sessionId));

                if (authSession == null)
                    return BadRequest();

                authSession.IsActive = false;
                authSession.LogoutTime = CallTime;
                DBSession.Update(authSession);

                transaction.Commit();

                return NoContent();
            }
        }
    }
}
