using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Utilities;
using CommandCentral.Framework.Data;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Authorization;
using CommandCentral.Enums;
using NHibernate.Linq;
using Microsoft.AspNetCore.JsonPatch;
using System.Linq.Expressions;

namespace CommandCentral.Controllers
{
    /// <summary>
    /// The person object is the central entry to a person's profile.  Permissions for each field can be attained from the /authorization controller.
    /// </summary>
    public partial class PersonsController : CommandCentralController
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
                    .Where(x => x.IsReleasableOutsideCoC == true)
                    .Select(item => new DTOs.PhysicalAddress.Get(item))
                    .ToList());
            }
            else
            {
                return Ok(items
                    .Select(item => new DTOs.PhysicalAddress.Get(item))
                    .ToList());
            }
        }

        [HttpGet("{personId}/PhysicalAddresses/{id}")]
        [RequireAuthentication]
        public IActionResult GetPhysicalAddress(Guid personId, Guid id)
        {
            var item = DBSession.Query<PhysicalAddress>().Where(x => x.Id == id && x.Person.Id == personId).FirstOrDefault();
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

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(GetPhysicalAddress), new { personId = person.Id, id = item.Id }, new DTOs.PhysicalAddress.Get(item));
        }

        [HttpPut("{personId}/PhysicalAddresses/{id}")]
        [RequireAuthentication]
        public IActionResult PutPhysicalAddress(Guid personId, Guid id, [FromBody] DTOs.PhysicalAddress.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var item = DBSession.Query<PhysicalAddress>().Where(x => x.Id == id && x.Person.Id == personId).FirstOrDefault();
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

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Update(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(GetPhysicalAddress), new { personId = item.Person.Id, id = item.Id }, new DTOs.PhysicalAddress.Get(item));
        }

        [HttpDelete("{personId}/PhysicalAddresses/{id}")]
        [RequireAuthentication]
        public IActionResult DeletePhysicalAddress(Guid personId, Guid id)
        {
            var item = DBSession.Query<PhysicalAddress>().Where(x => x.Id == id && x.Person.Id == personId).FirstOrDefault();
            if (item == null)
                return NotFound("A physical address with that id could not be found for a person with the given id.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.PhysicalAddresses))
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

