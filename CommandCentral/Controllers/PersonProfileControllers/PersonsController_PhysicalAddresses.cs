using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.PersonProfileControllers
{
    public partial class PersonsController
    {
        [HttpGet("{personId}/PhysicalAddresses")]
        [RequireAuthentication]
        public IActionResult GetPhysicalAddresses(Guid personId)
        {
            var items = DBSession.Query<PhysicalAddress>().Where(x => x.Person.Id == personId).ToList();
            if (!items.Any())
                return Ok(new List<DTOs.PhysicalAddress.Get>());

            if (!User.GetFieldPermissions<Person>(items.First().Person).CanReturn(x => x.PhysicalAddresses))
                return Forbid();

            if (!User.IsInChainOfCommand(items.First().Person))
            {
                return Ok(items
                    .Where(x => x.IsReleasableOutsideCoC)
                    .Select(item => new DTOs.PhysicalAddress.Get(item))
                    .ToList());
            }

            return Ok(items
                .Select(item => new DTOs.PhysicalAddress.Get(item))
                .ToList());
        }

        [HttpGet("{personId}/PhysicalAddresses/{id}")]
        [RequireAuthentication]
        public IActionResult GetPhysicalAddress(Guid personId, Guid id)
        {
            var item = DBSession.Query<PhysicalAddress>()
                .FirstOrDefault(x => x.Id == id && x.Person.Id == personId);

            if (item == null)
                return NotFound("A physical address with that id could not be found for a person with the given id.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanReturn(x => x.PhysicalAddresses))
                return Forbid();

            if (!User.IsInChainOfCommand(item.Person) && !item.IsReleasableOutsideCoC)
                return Forbid();

            return Ok(new DTOs.PhysicalAddress.Get(item));
        }

        [HttpPost("{personId}/PhysicalAddresses")]
        [RequireAuthentication]
        public IActionResult PostPhysicalAddress(Guid personId, [FromBody] DTOs.PhysicalAddress.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var person = DBSession.Get<Person>(personId);
            if (person == null)
                return NotFoundParameter(personId, nameof(personId));

            if (!User.GetFieldPermissions<Person>(person).CanEdit(x => x.PhysicalAddresses))
                return Forbid();

            var item = new PhysicalAddress
            {
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country,
                Id = Guid.NewGuid(),
                IsHomeAddress = dto.IsHomeAddress,
                IsReleasableOutsideCoC = dto.IsReleaseableOutsideCoC,
                Person = person,
                State = dto.State,
                ZipCode = dto.ZipCode
            };

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(item);

            CommitChanges();

            return CreatedAtAction(nameof(GetPhysicalAddress), new {personId = person.Id, id = item.Id},
                new DTOs.PhysicalAddress.Get(item));
        }

        [HttpPut("{personId}/PhysicalAddresses/{id}")]
        [RequireAuthentication]
        public IActionResult PutPhysicalAddress(Guid personId, Guid id, [FromBody] DTOs.PhysicalAddress.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var item = DBSession.Query<PhysicalAddress>().FirstOrDefault(x => x.Id == id && x.Person.Id == personId);
            if (item == null)
                return NotFound("A physical address with that id could not be found for a person with the given id.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.PhysicalAddresses))
                return Forbid();

            item.Address = dto.Address;
            item.City = dto.City;
            item.Country = dto.Country;
            item.IsHomeAddress = dto.IsHomeAddress;
            item.IsReleasableOutsideCoC = dto.IsReleaseableOutsideCoC;
            item.State = dto.State;
            item.ZipCode = dto.ZipCode;

            CommitChanges();

            return CreatedAtAction(nameof(GetPhysicalAddress), new {personId = item.Person.Id, id = item.Id},
                new DTOs.PhysicalAddress.Get(item));
        }

        [HttpDelete("{personId}/PhysicalAddresses/{id}")]
        [RequireAuthentication]
        public IActionResult DeletePhysicalAddress(Guid personId, Guid id)
        {
            var item = DBSession.Query<PhysicalAddress>().FirstOrDefault(x => x.Id == id && x.Person.Id == personId);
            if (item == null)
                return NotFound("A physical address with that id could not be found for a person with the given id.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.PhysicalAddresses))
                return Forbid();

            DBSession.Delete(item);

            CommitChanges();

            return NoContent();
        }
    }
}