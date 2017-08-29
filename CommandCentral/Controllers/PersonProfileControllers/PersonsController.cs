using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.Muster;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Enums;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.PersonProfileControllers
{
    /// <summary>
    /// The person object is the central entry to a person's profile.  Permissions for each field can be attained from the /authorization controller.
    /// </summary>
    public partial class PersonsController : CommandCentralController
    {
        [HttpPost("query")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Person.Get>))]
        public IActionResult Query([FromBody] DTOs.Person.Query dto, [FromQuery] int limit = 1000, [FromQuery] string orderBy = nameof(Person.LastName))
        {
            if (dto == null)
                return BadRequestDTONull();

            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var statusPeriodSearch = CommonQueryStrategies.GetTimeRangeQueryExpression<StatusPeriod>(y => y.Range, dto.StatusPeriod);

            var predicate = ((Expression<Func<Person, bool>>) null)
                .AddStringQueryExpression(x => x.FirstName, dto.FirstName)
                .AddStringQueryExpression(x => x.FirstName, dto.FirstName)
                .AddStringQueryExpression(x => x.LastName, dto.LastName)
                .AddStringQueryExpression(x => x.MiddleName, dto.MiddleName)
                .AddStringQueryExpression(x => x.SSN, dto.SSN)
                .AddStringQueryExpression(x => x.DoDId, dto.DoDId)
                .AddStringQueryExpression(x => x.Supervisor, dto.Supervisor)
                .AddStringQueryExpression(x => x.WorkCenter, dto.WorkCenter)
                .AddStringQueryExpression(x => x.WorkRoom, dto.WorkRoom)
                .AddStringQueryExpression(x => x.Shift, dto.Shift)
                .AddStringQueryExpression(x => x.JobTitle, dto.JobTitle)
                .AddReferenceListQueryExpression(x => x.Designation, dto.Designation)
                .AddReferenceListQueryExpression(x => x.DutyStatus, dto.DutyStatus)
                .AddReferenceListQueryExpression(x => x.UIC, dto.UIC)
                .AddReferenceListQueryExpression(x => x.Sex, dto.Sex)
                .AddReferenceListQueryExpression(x => x.Ethnicity, dto.Ethnicity)
                .AddReferenceListQueryExpression(x => x.ReligiousPreference, dto.ReligiousPreference)
                .AddReferenceListQueryExpression(x => x.BilletAssignment, dto.BilletAssignment)
                .AddDateTimeQueryExpression(x => x.DateOfArrival, dto.DateOfArrival)
                .AddDateTimeQueryExpression(x => x.DateOfBirth, dto.DateOfBirth)
                .AddDateTimeQueryExpression(x => x.DateOfDeparture, dto.DateOfDeparture)
                .AddDateTimeQueryExpression(x => x.EAOS, dto.EAOS)
                .AddDateTimeQueryExpression(x => x.PRD, dto.PRD);

            if (dto.StatusPeriod != null && !dto.StatusPeriod.HasNeither())
                predicate = predicate.NullSafeAnd(x => x.StatusPeriods.Any(statusPeriodSearch.Compile()));

            var result = DBSession.Query<Person>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .OrderBy(x => x.LastName)
                .Take(limit)
                .ToList()
                .Select(person =>
                {
                    var perms = User.GetFieldPermissions<Person>(person);
                    return new DTOs.Person.Get(person, perms);
                })
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Retrieves the person identified by the current login session.
        /// </summary>
        /// <returns></returns>
        [HttpGet("me")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.Person.Get))]
        public IActionResult GetMe()
        {
            var perms = User.GetFieldPermissions<Person>(User);

            return Ok(new DTOs.Person.Get(User, perms));
        }

        /// <summary>
        /// Retrieves the person identified by the given Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.Person.Get))]
        public IActionResult Get(Guid id)
        {
            var person = DBSession.Get<Person>(id);
            if (person == null)
                return NotFound();

            var perms = User.GetFieldPermissions<Person>(person);

            return Ok(new DTOs.Person.Get(person, perms));
        }

        /// <summary>
        /// Creates a new person.  Client must have access to the "CreatePerson" submodule.
        /// </summary>
        /// <param name="dto">The dto containing all of the information needed to create a person.</param>
        /// <returns></returns>
        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.Person.Get))]
        public IActionResult Post([FromBody] DTOs.Person.Post dto)
        {
            if (dto == null)
                return BadRequest();

            if (!User.CanAccessSubmodules(SubModules.CreatePerson))
                return Forbid();

            var person = new Person
            {
                Id = Guid.NewGuid(),
                DateOfArrival = dto.DateOfArrival,
                DateOfBirth = dto.DateOfBirth,
                UIC = DBSession.Get<UIC>(dto.UIC),
                Designation = DBSession.Get<Designation>(dto.Designation),
                Paygrade = DBSession.Get<Paygrade>(dto.Paygrade),
                Division = DBSession.Get<Division>(dto.Division),
                DoDId = dto.DoDId,
                SSN = dto.SSN,
                LastName = dto.LastName,
                FirstName = dto.FirstName,
                Sex = DBSession.Get<Sex>(dto.Sex),
                DutyStatus = DBSession.Get<DutyStatus>(dto.DutyStatus)
            };

            var result = person.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(person);
                transaction.Commit();
            }

            Events.EventManager.OnPersonCreated(new Events.Args.PersonCreatedEventArgs
            {
                CreatedBy = User,
                Person = person
            }, this);

            return CreatedAtAction(nameof(Get), new { id = person.Id }, new DTOs.Person.Get(person, User.GetFieldPermissions<Person>(person)));
        }
    }
}

