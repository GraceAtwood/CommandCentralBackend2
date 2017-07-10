using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Authentication;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticationController : CommandCentralController
    {
        [HttpPost("[action]")]
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

                    Response.Headers.Add("sessionId", new Microsoft.Extensions.Primitives.StringValues(ses.Id.ToString()));

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
