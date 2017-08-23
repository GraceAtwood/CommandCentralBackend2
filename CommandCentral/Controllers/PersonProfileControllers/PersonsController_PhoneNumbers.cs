using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.PersonProfileControllers
{
    public partial class PersonsController
    {
        [HttpGet("{personId}/PhoneNumbers")]
        [RequireAuthentication]
        public IActionResult GetPhoneNumbers(Guid personId)
        {
            var items = DBSession.Query<PhoneNumber>().Where(x => x.Person.Id == personId).ToList();
            if (!items.Any())
                return Ok(new List<DTOs.PhoneNumber.Get>());

            if (!User.GetFieldPermissions<Person>(items.First().Person).CanReturn(x => x.PhoneNumbers))
                return Forbid();

            if (!User.IsInChainOfCommand(items.First().Person))
            {
                return Ok(items
                    .Where(x => x.IsReleasableOutsideCoC)
                    .Select(item => new DTOs.PhoneNumber.Get(item))
                    .ToList());
            }
            
            return Ok(items
                .Select(item => new DTOs.PhoneNumber.Get(item))
                .ToList());
        }

        [HttpGet("{personId}/PhoneNumbers/{id}")]
        [RequireAuthentication]
        public IActionResult GetPhoneNumber(Guid personId, Guid id)
        {
            var item = DBSession.Query<PhoneNumber>().FirstOrDefault(x => x.Id == id && x.Person.Id == personId);
            if (item == null)
                return NotFound("A phone number with that id could not be found for a person with the given id.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanReturn(x => x.PhoneNumbers))
                return Forbid();

            if (!User.IsInChainOfCommand(item.Person) && !item.IsReleasableOutsideCoC)
                return Forbid();

            return Ok(new DTOs.PhoneNumber.Get(item));
        }

        [HttpPost("{personId}/PhoneNumbers")]
        [RequireAuthentication]
        public IActionResult PostPhoneNumber(Guid personId, [FromBody] DTOs.PhoneNumber.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var person = DBSession.Get<Person>(personId);
            if (person == null)
                return NotFoundParameter(personId, nameof(personId));

            var phoneType = DBSession.Get<PhoneNumberType>(dto.PhoneType);
            if (phoneType == null)
                return NotFoundParameter(dto.PhoneType, nameof(dto.PhoneType));

            if (!User.GetFieldPermissions<Person>(person).CanEdit(x => x.PhoneNumbers))
                return Forbid();

            var item = new PhoneNumber
            {
                Id = Guid.NewGuid(),
                IsPreferred = dto.IsPreferred,
                IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC,
                Person = person,
                Number = dto.Number,
                PhoneType = phoneType
            };

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(GetPhoneNumbers), new { personId = person.Id, id = item.Id }, new DTOs.PhoneNumber.Get(item));
        }

        [HttpPut("{personId}/PhoneNumbers/{id}")]
        [RequireAuthentication]
        public IActionResult PutPhoneNumber(Guid personId, Guid id, [FromBody] DTOs.PhoneNumber.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var item = DBSession.Query<PhoneNumber>().FirstOrDefault(x => x.Id == id && x.Person.Id == personId);
            if (item == null)
                return NotFound("A phone number with that id could not be found for a person with the given id.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.PhoneNumbers))
                return Forbid();

            var phoneType = DBSession.Get<PhoneNumberType>(dto.PhoneType);
            if (phoneType == null)
                return NotFoundParameter(dto.PhoneType, nameof(dto.PhoneType));

            item.Number = dto.Number;
            item.IsPreferred = dto.IsPreferred;
            item.IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC;
            item.PhoneType = phoneType;

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Update(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(GetPhoneNumber), new { personId = item.Person.Id, id = item.Id }, new DTOs.PhoneNumber.Get(item));
        }

        [HttpDelete("{personId}/PhoneNumbers/{id}")]
        [RequireAuthentication]
        public IActionResult DeletePhoneNumber(Guid personId, Guid id)
        {
            var item = DBSession.Query<PhoneNumber>().FirstOrDefault(x => x.Id == id && x.Person.Id == personId);
            if (item == null)
                return NotFound("A phone number with that id could not be found for a person with the given id.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.PhoneNumbers))
                return Forbid();

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Delete(item);
                transaction.Commit();
            }

            return NoContent();
        }
    }
}

