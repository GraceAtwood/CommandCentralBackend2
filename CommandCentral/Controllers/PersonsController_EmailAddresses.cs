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
        [HttpGet("{personId}/EmailAddresses")]
        [RequireAuthentication]
        public IActionResult GetEmailAddresses(Guid personId)
        {
            var items = DBSession.Query<EmailAddress>().Where(x => x.Person.Id == personId).ToList();
            if (!items.Any())
                return Ok(new List<DTOs.EmailAddress.Get>());

            if (!User.GetFieldPermissions<Person>(items.First().Person).CanReturn(x => x.EmailAddresses))
                return Forbid();

            if (!User.IsInChainOfCommand(items.First().Person))
            {
                return Ok(items
                    .Where(x => x.IsReleasableOutsideCoC == true)
                    .Select(item => new DTOs.EmailAddress.Get(item))
                    .ToList());
            }
            else
            {
                return Ok(items
                    .Select(item => new DTOs.EmailAddress.Get(item))
                    .ToList());
            }
        }

        [HttpGet("{personId}/EmailAddresses/{id}")]
        [RequireAuthentication]
        public IActionResult GetEmailAddress(Guid personId, Guid id)
        {
            var item = DBSession.Query<EmailAddress>().Where(x => x.Id == id && x.Person.Id == personId).FirstOrDefault();
            if (item == null)
                return NotFound("An email address with that id could not be found for a person with the given id.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanReturn(x => x.EmailAddresses))
                return Forbid();

            if (!User.IsInChainOfCommand(item.Person) && !item.IsReleasableOutsideCoC)
                return Forbid();

            return Ok(new DTOs.EmailAddress.Get(item));
        }

        [HttpPost("{personId}/EmailAddresses")]
        [RequireAuthentication]
        public IActionResult PostEmailAddress(Guid personId, [FromBody] DTOs.EmailAddress.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var person = DBSession.Get<Person>(personId);
            if (person == null)
                return NotFoundParameter(personId, nameof(personId));

            if (!User.GetFieldPermissions<Person>(person).CanEdit(x => x.EmailAddresses))
                return Forbid();

            var item = new EmailAddress
            {
                Address = dto.Address,
                Id = Guid.NewGuid(),
                IsPreferred = dto.IsPreferred,
                IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC,
                Person = person
            };

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(GetEmailAddress), new { personId = person.Id, id = item.Id }, new DTOs.EmailAddress.Get(item));
        }

        [HttpPut("{personId}/EmailAddresses/{id}")]
        [RequireAuthentication]
        public IActionResult PutEmailAddress(Guid personId, Guid id, [FromBody] DTOs.EmailAddress.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var item = DBSession.Query<EmailAddress>().Where(x => x.Id == id && x.Person.Id == personId).FirstOrDefault();
            if (item == null)
                return NotFound("An email address with that id could not be found for a person with the given id.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.EmailAddresses))
                return Forbid();

            item.Address = dto.Address;
            item.IsPreferred = dto.IsPreferred;
            item.IsReleasableOutsideCoC = dto.IsReleasableOutsideCoC;

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Update(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(GetEmailAddress), new { personId = item.Person.Id, id = item.Id }, new DTOs.EmailAddress.Get(item));
        }

        [HttpDelete("{personId}/EmailAddresses/{id}")]
        [RequireAuthentication]
        public IActionResult DeleteEmailAddress(Guid personId, Guid id)
        {
            var item = DBSession.Query<EmailAddress>().Where(x => x.Id == id && x.Person.Id == personId).FirstOrDefault();
            if (item == null)
                return NotFound("An email address with that id could not be found for a person with the given id.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.EmailAddresses))
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

