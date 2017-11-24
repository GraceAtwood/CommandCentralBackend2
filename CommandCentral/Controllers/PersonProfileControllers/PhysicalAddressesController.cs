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
    public class PhysicalAddressesController : CommandCentralController
    {
        /// <summary>
        /// Queries  the physical addresses collection.
        /// </summary>
        /// <param name="person">A person query for the person who owns a physical address.</param>
        /// <param name="isReleasableOutsideCoC">A boolean query for if a physical address is releasable outside the person's chain of command.</param>
        /// <param name="limit">[Default: 1000] [Optional] Instructs the service to return no more than this number of results.  Because this 
        /// endpoint does not support an order by parameter, the result set will be arbitrarily ordered and truncated.</param>
        /// <param name="isHomeAddress">A boolean query for if the address is the client's home address.</param>
        /// <param name="zipCode">A string query for the zip code.</param>
        /// <param name="city">A string query for the city.</param>
        /// <param name="state">A string query for the state.</param>
        /// <param name="country">A string query for the country.</param>
        /// <param name="address">A string query for the address.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(DTOs.PhoneNumber.Get))]
        public IActionResult Get([FromQuery] string person, [FromQuery] string address, [FromQuery] string city,
            [FromQuery] string state, [FromQuery] string country, [FromQuery] bool? isHomeAddress,
            [FromQuery] string zipCode, [FromQuery] bool? isReleasableOutsideCoC, [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var predicate = ((Expression<Func<PhysicalAddress, bool>>) null)
                .AddStringQueryExpression(x => x.Address, address)
                .AddStringQueryExpression(x => x.State, state)
                .AddStringQueryExpression(x => x.ZipCode, zipCode)
                .AddStringQueryExpression(x => x.Country, country)
                .AddStringQueryExpression(x => x.City, city)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddNullableBoolQueryExpression(x => x.IsHomeAddress, isHomeAddress)
                .AddNullableBoolQueryExpression(x => x.IsReleasableOutsideCoC, isReleasableOutsideCoC);

            var physicalAddresses = DBSession.Query<PhysicalAddress>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .Take(limit)
                .ToList();

            var checkedPersons = new Dictionary<Person, bool>();
            foreach (var physicalAddress in physicalAddresses.Where(x => !x.IsReleasableOutsideCoC))
            {
                if (!checkedPersons.TryGetValue(physicalAddress.Person, out var canView))
                {
                    checkedPersons[physicalAddress.Person] = User.IsInChainOfCommand(physicalAddress.Person);
                    canView = checkedPersons[physicalAddress.Person];
                }

                if (!canView)
                {
                    physicalAddress.Address = "REDACTED";
                    physicalAddress.State = "REDACTED";
                    physicalAddress.City = "REDACTED";
                    physicalAddress.Country = "REDACTED";
                    physicalAddress.ZipCode = "REDACTED";
                }
            }
            
            //Now that we've scrubbed out all the values we don't want to expose to the client, 
            //we're ready to send the results out.
            var results = physicalAddresses.Select(x => new DTOs.PhysicalAddress.Get(x)).ToList();
            return Ok(results);
        }
    }
}