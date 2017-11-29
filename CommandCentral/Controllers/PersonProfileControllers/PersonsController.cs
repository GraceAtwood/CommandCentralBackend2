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
    public class PersonsController : CommandCentralController
    {
        /// <summary>
        /// Queries the persons collection.  Results are passed through a permissions filter prior to serving them to the client.  
        /// Large result sets could result in longer load times.  
        /// Properties your client is not able to view (such as SSN, possibly) will be replaced with the default value for the type of that property (null in the case of SSN).  
        /// You are responsible for knowing which properties your client can or can not view.  
        /// </summary>
        /// <param name="firstName">A string quuery for the first name of a person.</param>
        /// <param name="lastName">A string query for the last name of a person.</param>
        /// <param name="middleName">A string query for the middle name of a person.</param>
        /// <param name="ssn">A string query for the ssn of a person.</param>
        /// <param name="dodId">A string query for the dod id of a person.</param>
        /// <param name="supervisor">A string query for a person's supervisor.</param>
        /// <param name="workCenter">A string query for a person's work center.</param>
        /// <param name="workRoom">A string query for a person's work room.</param>
        /// <param name="shift">A string query for a person's shift.</param>
        /// <param name="jobTitle">A string query for a person's job title.</param>
        /// <param name="designation">A reference list query for a person's designation.</param>
        /// <param name="dutyStatus">An enum query for a person's duty status.</param>
        /// <param name="uic">A reference list query for a person's UIC.</param>
        /// <param name="sex">An enum query for a person's sex.</param>
        /// <param name="ethnicity">A reference list query for a person's ethnicity.</param>
        /// <param name="religiousPreference">A reference list query for a person's religious preference.</param>
        /// <param name="billetAssignment">An enum query for a person's billet assignment.</param>
        /// <param name="dateOfArrival">A time range query for a person's date of arrival.</param>
        /// <param name="dateOfBirth">A time range query for a person's date of birth.</param>
        /// <param name="dateOfDeparture">A time range query for a person's date of departure.</param>
        /// <param name="eaos">A time range query for a person's EAOS.</param>
        /// <param name="prd">A time range query for a person's PRD.</param>
        /// <param name="statusPeriod">A time range query for any status period that falls within that range belonging to a person</param>
        /// <param name="limit">[Default: 1000][Optional] Instructs the service to limit its results to the given limit number.</param>
        /// <param name="orderBy">[Default: LastName][Optional] Instructs the service to order the results by the given property.  
        /// Not all properties are supported.  This parameter is not meant to offload ordering work to the API; 
        /// rather, it is meant to be used in conjunction with the limit parameter to get only those results the client desires.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Person.Get>))]
        public IActionResult Get([FromQuery] string firstName, [FromQuery] string lastName,
            [FromQuery] string middleName, [FromQuery] string dodId,
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
                .Select(person => new DTOs.Person.Get(User, person))
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
            return Ok(new DTOs.Person.Get(User, User));
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
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.Person.Get(User, person));
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
                return BadRequestDTONull();

            if (!User.SpecialPermissions.Contains(SpecialPermissions.CreatePerson))
                return Forbid("You must have access to the Create Persons sub module to create a person.");

            var uic = DBSession.Get<UIC>(dto.UIC);
            if (uic == null)
                return NotFoundParameter(dto.UIC, nameof(dto.UIC));

            var division = DBSession.Get<Division>(dto.Division);
            if (division == null)
                return NotFoundParameter(dto.Division, nameof(dto.Division));

            var designation = DBSession.Get<Designation>(dto.Designation);
            if (designation == null)
                return NotFoundParameter(dto.Designation, nameof(dto.Designation));

            var person = new Person
            {
                Id = Guid.NewGuid(),
                DateOfArrival = dto.DateOfArrival,
                DateOfBirth = dto.DateOfBirth,
                UIC = uic,
                Designation = designation,
                Paygrade = dto.Paygrade,
                Division = division,
                DoDId = dto.DoDId,
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

            return CreatedAtAction(nameof(Get), new {id = person.Id}, new DTOs.Person.Get(User, person));
        }
    }
}