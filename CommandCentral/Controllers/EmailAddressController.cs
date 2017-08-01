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
            if (Person == Guid.Empty)
                return BadRequest("Query string with a person id is missing or malformed. Loading all email addresses is not allowed.");

            var items = DBSession.QueryOver<EmailAddress>().Where(x => x.Person.Id == Person).List();

            if (!items.Any())
                return NotFound();

            IEnumerable<EmailAddress> result;
            if (!User.IsInChainOfCommand(items.First().Person))
            {
                result = items.Where(x => x.IsReleasableOutsideCoC);
            }
            else
            {
                result = items;
            }

            return Ok(result.Select(x =>
                new DTOs.EmailAddress.Get
                {
                    Address = x.Address,
                    IsReleasableOutsideCoC = x.IsReleasableOutsideCoC,
                    Id = x.Id,
                    IsPreferred = x.IsPreferred,
                    Person = x.Person.Id
                })
            );
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<EmailAddress>(id);
            if (item == null)
                return NotFound();
            
            if (item.IsReleasableOutsideCoC || User.IsInChainOfCommand(item.Person))
            {
                return Ok(new DTOs.EmailAddress.Get
                {
                    Id = item.Id,
                    Address = item.Address,
                    IsReleasableOutsideCoC = item.IsReleasableOutsideCoC,
                    Person = item.Person.Id,
                    IsPreferred = item.IsPreferred
                });
            }

            return Unauthorized();
        }

        [HttpPost]
        [RequireAuthentication]
        public IActionResult Post([FromBody]DTOs.EmailAddress.Update dto)
        {
            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFound();

            if (!User.GetFieldPermissions<Person>(person).CanEdit(x => x.EmailAddresses))
                return PermissionDenied();

            using (var transaction = DBSession.BeginTransaction())
            {
                var item = new EmailAddress
                {
                    Id = Guid.NewGuid(),
                    Address = dto.Address,
                    IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC,
                    Person = person,
                    IsPreferred = dto.IsPreferred
                };

                var result = new EmailAddress.Validator().Validate(item);
                if (!result.IsValid)
                {
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));
                }

                DBSession.Save(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.EmailAddress.Get
                {
                    Id = item.Id,
                    Address = item.Address,
                    IsReleasableOutsideCoC = item.IsReleasableOutsideCoC,
                    Person = item.Person.Id,
                    IsPreferred = item.IsPreferred
                });
            }

        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        public IActionResult Put(Guid id, [FromBody]DTOs.EmailAddress.Update dto)
        {
            using (var transaction = DBSession.BeginTransaction())
            {
                var item = DBSession.Get<EmailAddress>(id);
                if (item == null)
                    return NotFound();

                if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.EmailAddresses))
                    return PermissionDenied();

                item.Address = dto.Address;
                item.IsPreferred = dto.IsPreferred;
                item.IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC;

                var result = new EmailAddress.Validator().Validate(item);
                if (!result.IsValid)
                {
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));
                }

                DBSession.Update(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Put), new { id = item.Id }, new DTOs.EmailAddress.Get
                {
                    Id = item.Id,
                    Address = item.Address,
                    IsReleasableOutsideCoC = item.IsReleasableOutsideCoC,
                    Person = item.Person.Id,
                    IsPreferred = item.IsPreferred
                });
            }
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        public IActionResult Delete(Guid id)
        {
            using (var transaction = DBSession.BeginTransaction())
            {
                var item = DBSession.Get<EmailAddress>(id);

                if (item == null)
                    return NotFound();

                if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.EmailAddresses))
                    return PermissionDenied();

                DBSession.Delete(item);
                transaction.Commit();
                return Ok();
            }
        }
    }
}
