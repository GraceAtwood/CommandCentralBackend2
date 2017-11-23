using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.PersonProfileControllers
{
    /// <summary>
    /// Provides access to the email addresses resource.
    /// </summary>
    public class EmailAddressesController : CommandCentralController
    {
        /// <summary>
        /// Provides querying of the email addresses collection.  
        /// If your client is not in a person's chain of command and the email address is not marked as releasable outside the CoC, 
        /// the address property will be replaced with 'REDACTED'.
        /// </summary>
        /// <param name="person">A person query for the owner of the email address.</param>
        /// <param name="address">A string query for the address itself.</param>
        /// <param name="isReleasableOutsideCoC">A bool query for if the owner of the email address 
        /// has indicated their email address can be released outside their chain of command.</param>
        /// <param name="isPreferred">A bool query for if the client would prefer to be contacted at this email address.</param>
        /// <param name="limit">[Default: 1000] [Optional] Instructs the service to return no more than this number of results.  Because this 
        /// endpoint does not support an order by parameter, the result set will be arbitrarily ordered and truncated.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(DTOs.EmailAddress.Get))]
        public IActionResult Get([FromQuery] string person, [FromQuery] string address,
            [FromQuery] bool? isReleasableOutsideCoC, [FromQuery] bool? isPreferred, [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));
            
            var predicate = ((Expression<Func<EmailAddress, bool>>) null)
                .AddStringQueryExpression(x => x.Address, address)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddNullableBoolQueryExpression(x => x.IsPreferred, isPreferred)
                .AddNullableBoolQueryExpression(x => x.IsReleasableOutsideCoC, isReleasableOutsideCoC);

            var emailAddresses = DBSession.Query<EmailAddress>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .Take(limit)
                .ToList();

            var checkedPersons = new Dictionary<Person, bool>();
            foreach (var emailAddress in emailAddresses)
            {
                if (emailAddress.IsReleasableOutsideCoC) 
                    continue;
                
                if (!checkedPersons.TryGetValue(emailAddress.Person, out var canView))
                {
                    checkedPersons[emailAddress.Person] = User.IsInChainOfCommand(emailAddress.Person);
                    canView = checkedPersons[emailAddress.Person];
                }

                if (!canView)
                    emailAddress.Address = "REDACTED";
            }
            
            //Now that we've scrubbed out all the values we don't want to expose to the client, 
            //we're ready to send the results out.
            var results = emailAddresses.Select(x => new DTOs.EmailAddress.Get(x)).ToList();
            return Ok(results);
        }
    }
}