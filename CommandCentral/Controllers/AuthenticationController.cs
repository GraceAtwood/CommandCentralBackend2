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
    [Route("api/[controller]")]
    public class AuthenticationController : CommandCentralController
    {
        [HttpPost]
        public IActionResult Login([FromBody] DTOs.LoginRequestDTO dto)
        {
            using (var transaction = DBSession.BeginTransaction())
            {
                try
                {
                    var person = DBSession.QueryOver<Person>().Where(x => x.Username == dto.Username).SingleOrDefault();

                    if (person == null)
                        return Unauthorized();

                    if (!PasswordHash.ValidatePassword(dto.Password, person.PasswordHash))
                    {

                        if (person.EmailAddresses.Any())
                        {
                            var model = new Email.Models.FailedAccountLoginEmailModel
                            {
                                FriendlyName = person.ToString()
                            };

                            //Ok, so we have an email we can use to contact the person!
                            Email.EmailInterface.CCEmailMessage
                                .CreateDefault()
                                .To(person.EmailAddresses.Select(x => new System.Net.Mail.MailAddress(x.Address, person.ToString())))
                                .Subject("Security Alert : Failed Login")
                                .HTMLAlternateViewUsingTemplateFromEmbedded("CommandCentral.Email.Templates.FailedAccountLogin_HTML.html", model)
                                .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));

                            //Now we also need to add the event to client's account history.
                            person.AccountHistory.Add(new AccountHistoryEvent
                            {
                                AccountHistoryEventType = ReferenceListHelper<AccountHistoryType>.Find("Failed Login"),
                                EventTime = this.CallTime
                            });

                            DBSession.Update(person);
                        }

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
                        EventTime = CallTime
                    });

                    Response.Headers.Add("Access-Control-Expose-Headers", "sessionid");
                    Response.Headers.Add("sessionid", new Microsoft.Extensions.Primitives.StringValues(ses.Id.ToString()));

                    transaction.Commit();

                    return Ok();
                }
                catch (Exception e)
                {
                    LogException(e);
                    transaction.Rollback();
                    return StatusCode(500);
                }
            }
        }

        [HttpDelete]
        public IActionResult Logout([FromHeader] string sessionId)
        {
            using (var transaction = DBSession.BeginTransaction())
            {
                try
                {

                    var authSession = DBSession.Get<AuthenticationSession>(Guid.Parse(sessionId));

                    if (authSession == null)
                        throw new Exception("Authentication session was null.");

                    authSession.IsActive = false;
                    authSession.LogoutTime = CallTime;
                    DBSession.Update(authSession);

                    transaction.Commit();
                    return Ok();
                }
                catch (Exception e)
                {
                    LogException(e);
                    transaction.Rollback();
                    return StatusCode(500);
                }
            }
        }
    }
}
