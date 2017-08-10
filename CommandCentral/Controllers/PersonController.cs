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

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class PersonController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        public IActionResult Get()
        {
            throw new NotImplementedException();
        }

        [HttpGet("me")]
        [RequireAuthentication]
        public IActionResult GetMe()
        {
            var perms = User.GetFieldPermissions<Person>(User);

            return Ok(new DTOs.Person.Get(User, perms));
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid id)
        {
            var person = DBSession.Get<Person>(id);
            if (person == null)
                return NotFound();

            var perms = User.GetFieldPermissions<Person>(person);

            return Ok(new DTOs.Person.Get(person, perms));
        }

        [HttpPost]
        [RequireAuthentication]
        public IActionResult Post([FromBody] DTOs.Person.Post dto)
        {
            if (dto == null)
                return BadRequest();

            if (!User.CanAccessSubmodules(SubModules.CreatePerson))
                Forbid();

            var person = new Person
            {
                Id = Guid.NewGuid(),
                DateOfArrival = dto.DateOfArrival,
                DateOfBirth = dto.DateOfBirth,
                UIC = ReferenceListHelper<UIC>.Get(dto.UIC),
                Designation = ReferenceListHelper<Designation>.Get(dto.Designation),
                Paygrade = ReferenceListHelper<Paygrade>.Get(dto.Paygrade),
                Division = DBSession.Get<Division>(dto.Division),
                DoDId = dto.DoDId,
                SSN = dto.SSN,
                LastName = dto.LastName,
                FirstName = dto.FirstName,
                Sex = ReferenceListHelper<Sex>.Get(dto.Sex),
                DutyStatus = ReferenceListHelper<DutyStatus>.Get(dto.DutyStatus)
            };

            var result = person.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(person);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(Get), new { id = person.Id }, new DTOs.Person.Get(person, User.GetFieldPermissions<Person>(person)));
        }

        [HttpPatch("{id}")]
        [RequireAuthentication]
        public IActionResult Patch(Guid id, [FromBody]JsonPatchDocument<Person> personPatchDocument)
        {
            throw new NotImplementedException();
        }
    }
}

