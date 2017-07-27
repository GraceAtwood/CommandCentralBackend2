using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Authorization;
using CommandCentral.DTOs;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class EmailAddressController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        public IActionResult GetByPerson([FromQuery]Guid Person)
        {
            var items = DBSession.QueryOver<EmailAddress>().Where(x => x.Person.Id == Person).List();

            if (!items.Any())
                return NotFound();

            var isInChainOfCommand = new ResolvedPermissions(User, items.First().Person).IsInChainOfCommand.Any(x => x.Value);

            IEnumerable<EmailAddress> result;
            if (!isInChainOfCommand)
            {
                result = items.Where(x => x.IsReleasableOutsideCoC);
            }
            else
            {
                result = items;
            }

            return Ok(result.Select(x =>
                new EmailAddressDTO
                {
                    Address = x.Address,
                    IsReleasableOutsideCoC = x.IsReleasableOutsideCoC,
                    Id = x.Id,
                    IsPreferred = x.IsPreferred,
                    Person = x.Person.Id
                })
            );
        }

        [HttpPost]
        [RequireAuthentication]
        public void Post([FromBody]string value)
        {



        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
