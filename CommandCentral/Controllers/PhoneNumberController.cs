using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Authorization;
using CommandCentral.DTOs;
using CommandCentral.Entities.ReferenceLists;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class PhoneNumberController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        public IActionResult GetByPerson([FromQuery]Guid Person)
        {
            if (Person == Guid.Empty)
                return BadRequest("Query string with a person id is missing or malformed. Loading all phone numbers is not allowed.");

            var items = DBSession.QueryOver<PhoneNumber>().Where(x => x.Person.Id == Person).List();

            if (!items.Any())
                return NotFound();

            IEnumerable<PhoneNumber> result;
            if (!User.IsInChainOfCommand(items.First().Person))
            {
                result = items.Where(x => x.IsReleasableOutsideCoC);
            }
            else
            {
                result = items;
            }

            return Ok(result.Select(x =>
                new DTOs.PhoneNumber.Get
                {
                    Number = x.Number,
                    IsReleasableOutsideCoC = x.IsReleasableOutsideCoC,
                    Id = x.Id,
                    IsPreferred = x.IsPreferred,
                    Person = x.Person.Id,
                    PhoneType = x.PhoneType.Id
                })
            );
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<PhoneNumber>(id);
            if (item == null)
                return NotFound();

            if (item.IsReleasableOutsideCoC || User.IsInChainOfCommand(item.Person))
            {
                return Ok(new DTOs.PhoneNumber.Get
                {
                    Id = item.Id,
                    Number = item.Number,
                    PhoneType = item.PhoneType.Id,
                    IsReleasableOutsideCoC = item.IsReleasableOutsideCoC,
                    Person = item.Person.Id,
                    IsPreferred = item.IsPreferred
                });
            }

            return Forbid();
        }

        [HttpPost]
        [RequireAuthentication]
        public IActionResult Post([FromBody]DTOs.PhoneNumber.Update dto)
        {
            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFound();

            if (!User.GetFieldPermissions<Person>(person).CanEdit(x => x.EmailAddresses))
                return Forbid();

            var phoneType = DBSession.Get<PhoneNumberType>(dto.PhoneType);
            if (phoneType == null)
                return NotFound($"The parameter {nameof(dto.PhoneType)} could not be found.");

            using (var transaction = DBSession.BeginTransaction())
            {
                var item = new PhoneNumber
                {
                    Id = Guid.NewGuid(),
                    Number = dto.Number,
                    IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC,
                    Person = person,
                    IsPreferred = dto.IsPreferred,
                    PhoneType = phoneType
                };

                var result = item.Validate();
                if (!result.IsValid)
                {
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));
                }

                DBSession.Save(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.PhoneNumber.Get
                {
                    Id = item.Id,
                    Number = item.Number,
                    PhoneType = item.PhoneType.Id,
                    IsReleasableOutsideCoC = item.IsReleasableOutsideCoC,
                    Person = item.Person.Id,
                    IsPreferred = item.IsPreferred
                });
            }

        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        public IActionResult Put(Guid id, [FromBody]DTOs.PhoneNumber.Update dto)
        {
            using (var transaction = DBSession.BeginTransaction())
            {
                var item = DBSession.Get<PhoneNumber>(id);
                if (item == null)
                    return NotFound();

                if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.EmailAddresses))
                    return Forbid();

                var phoneType = DBSession.Get<PhoneNumberType>(dto.PhoneType);
                if (phoneType == null)
                    return NotFound($"The object identified by the parameter {nameof(dto.PhoneType)} could not be found.");

                item.IsPreferred = dto.IsPreferred;
                item.IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC;
                item.Number = dto.Number;
                item.PhoneType = phoneType;

                var result = item.Validate();
                if (!result.IsValid)
                {
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));
                }

                DBSession.Update(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Put), new { id = item.Id }, new DTOs.PhoneNumber.Get
                {
                    Id = item.Id,
                    Number = item.Number,
                    PhoneType = item.PhoneType.Id,
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
                var item = DBSession.Get<PhoneNumber>(id);

                if (item == null)
                    return NotFound();

                if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.PhoneNumbers))
                    return Forbid();

                DBSession.Delete(item);
                transaction.Commit();
                return Ok();
            }
        }
    }
}
