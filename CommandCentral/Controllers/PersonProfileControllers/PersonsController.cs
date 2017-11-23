using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.DTOs;
using CommandCentral.Entities;
using CommandCentral.Entities.Muster;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Enums;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.PersonProfileControllers
{
    /// <summary>
    /// The person object is the central entry to a person's profile.  Permissions for each field can be attained from the /authorization controller.
    /// </summary>
    public partial class PersonsController : CommandCentralController
    {
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Person.Get>))]
        public IActionResult Get([FromQuery] string firstName, [FromQuery] string lastName,
            [FromQuery] string middleName, [FromQuery] string ssn, [FromQuery] string dodId,
            [FromQuery] string supervisor, [FromQuery] string workCenter, [FromQuery] string workRoom,
            [FromQuery] string shift, [FromQuery] string jobTitle, [FromQuery] string designation,
            [FromQuery] string dutyStatus, [FromQuery] string uic, [FromQuery] string sex, [FromQuery] string ethnicity,
            [FromQuery] string religiousPreference, [FromQuery] string billetAssignment,
            [FromQuery] DateTimeRangeQuery dateOfArrival, [FromQuery] DateTimeRangeQuery dateOfBirth,
            [FromQuery] DateTimeRangeQuery dateOfDeparture, [FromQuery] DateTimeRangeQuery eaos,
            [FromQuery] DateTimeRangeQuery prd, [FromQuery] DateTimeRangeQuery statusPeriod,
            [FromQuery] int limit = 1000, [FromQuery] string orderBy = nameof(Person.LastName))
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var statusPeriodSearch =
                CommonQueryStrategies.GetTimeRangeQueryExpression<StatusPeriod>(y => y.Range, statusPeriod);

            var predicate = ((Expression<Func<Person, bool>>) null)
                .AddStringQueryExpression(x => x.FirstName, firstName)
                .AddStringQueryExpression(x => x.LastName, lastName)
                .AddStringQueryExpression(x => x.MiddleName, middleName)
                .AddStringQueryExpression(x => x.SSN, ssn)
                .AddStringQueryExpression(x => x.DoDId, dodId)
                .AddStringQueryExpression(x => x.Supervisor, supervisor)
                .AddStringQueryExpression(x => x.WorkCenter, workCenter)
                .AddStringQueryExpression(x => x.WorkRoom, workRoom)
                .AddStringQueryExpression(x => x.Shift, shift)
                .AddStringQueryExpression(x => x.JobTitle, jobTitle)
                .AddReferenceListQueryExpression(x => x.Designation, designation)
                .AddPartialEnumQueryExpression(x => x.DutyStatus, dutyStatus)
                .AddReferenceListQueryExpression(x => x.UIC, uic)
                .AddPartialEnumQueryExpression(x => x.Sex, sex)
                .AddReferenceListQueryExpression(x => x.Ethnicity, ethnicity)
                .AddReferenceListQueryExpression(x => x.ReligiousPreference, religiousPreference)
                .AddPartialEnumQueryExpression(x => x.BilletAssignment, billetAssignment)
                .AddDateTimeQueryExpression(x => x.DateOfArrival, dateOfArrival)
                .AddDateTimeQueryExpression(x => x.DateOfBirth, dateOfBirth)
                .AddDateTimeQueryExpression(x => x.DateOfDeparture, dateOfDeparture)
                .AddDateTimeQueryExpression(x => x.EAOS, eaos)
                .AddDateTimeQueryExpression(x => x.PRD, prd);

            if (statusPeriod != null && statusPeriod.HasBoth())
                predicate = predicate.NullSafeAnd(x => x.StatusPeriods.Any(statusPeriodSearch.Compile()));

            Expression<Func<Person, object>> orderBySelector;

            if (String.IsNullOrWhiteSpace(orderBy))
                return BadRequest("The order by parameter may not be empty.  " +
                                  "You may omit the parameter but do not send an empty string.");

            switch (orderBy)
            {
                case nameof(Person.LastName):
                {
                    orderBySelector = x => x.LastName;
                    break;
                }
                case nameof(Person.DateOfDeparture):
                {
                    orderBySelector = x => x.DateOfDeparture;
                    break;
                }
                case nameof(Person.DateOfArrival):
                {
                    orderBySelector = x => x.DateOfArrival;
                    break;
                }
                case nameof(Person.DateOfBirth):
                case nameof(Person.Age):
                {
                    orderBySelector = x => x.DateOfBirth;
                    break;
                }
                case nameof(Person.EAOS):
                {
                    orderBySelector = x => x.EAOS;
                    break;
                }
                default:
                    return BadRequest("We do not support that property as an order by parameter.");
            }

            var result = DBSession.Query<Person>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .OrderBy(orderBySelector)
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
                Paygrade = dto.Paygrade,
                Division = DBSession.Get<Division>(dto.Division),
                DoDId = dto.DoDId,
                SSN = dto.SSN,
                LastName = dto.LastName,
                FirstName = dto.FirstName,
                Sex = dto.Sex,
                DutyStatus = dto.DutyStatus
            };

            var result = person.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(person);

            CommitChanges();

            Events.EventManager.OnPersonCreated(new Events.Args.PersonCreatedEventArgs
            {
                CreatedBy = User,
                Person = person
            }, this);

            return CreatedAtAction(nameof(Get), new {id = person.Id},
                new DTOs.Person.Get(person, User.GetFieldPermissions<Person>(person)));
        }
    }
}