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
        [HttpGet("advanced")]
        [ProducesResponseType(typeof(List<DTOs.Person.Get>), 200)]
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
                    return BadRequest($"We do not support that property as an order by parameter. Property: {orderBy}");
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
        /// Queries the persons collection using the simple search algorithm, optionally constraining the results by duty status and command.    
        /// If you omit everything, it'll load every person in the database.
        /// </summary>
        /// <param name="searchValue">The value to search for.  Example: e5 40533 cti will find all those sailors.  
        /// First name, last name, dod id, division, uic, designation, and paygrade will all be searched using a wild card contains search.</param>
        /// <param name="dutyStatus">An exact enum query for the duty status of a person.</param>
        /// <param name="command">A command query for the command of a person.</param>
        /// <param name="limit">[Default = 1000] Instructs the service to return no more than this many results.</param>
        /// <param name="orderBy">[Default = LastName] Instructs the service to order the results by the given parameter.</param>
        /// <returns></returns>
        [HttpGet("simple")]
        [ProducesResponseType(typeof(List<DTOs.Person.Get>), 200)]
        public IActionResult Get([FromQuery] string searchValue, [FromQuery] string dutyStatus,
            [FromQuery] string command, [FromQuery] int limit = 1000,
            [FromQuery] string orderBy = nameof(Person.LastName))
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            Expression<Func<Person, bool>> predicate = null;

            foreach (var term in searchValue.SplitByOr())
            {
                var matchedPaygrades = EnumUtilities.GetPartialValueMatches<Paygrades>(term).ToList();

                predicate = predicate.NullSafeAnd(x =>
                    x.FirstName.Contains(term) || x.LastName.Contains(term) || x.DoDId == term ||
                    x.Division.Name.Contains(term) || x.UIC.Value.Contains(term) ||
                    x.Designation.Value.Contains(term) || matchedPaygrades.Contains(x.Paygrade));
            }

            predicate = predicate
                .AddExactEnumQueryExpression(x => x.DutyStatus, dutyStatus)
                .AddCommandQueryExpression(x => x.Division.Department.Command, command);

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
                    return BadRequest($"We do not support that property as an order by parameter. Property: {orderBy}");
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
        [ProducesResponseType(typeof(DTOs.Person.Get), 200)]
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
        [ProducesResponseType(typeof(DTOs.Person.Get), 200)]
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
        [ProducesResponseType(typeof(DTOs.Person.Get), 201)]
        public IActionResult Post([FromBody] DTOs.Person.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.SpecialPermissions.Contains(SpecialPermissions.CreatePerson))
                return Forbid("You must have access to the Create Persons sub module to create a person.");

            if (!TryGet(dto.UIC, out UIC uic))
                return NotFoundParameter(dto.UIC, nameof(dto.UIC));

            if (!TryGet(dto.Division, out Division division))
                return NotFoundParameter(dto.Division, nameof(dto.Division));

            if (!TryGet(dto.Designation, out Designation designation))
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
                return BadRequestWithValidationErrors(result);

            Save(person);
            LogEntityCreation(person);
            CommitChanges();

            Events.EventManager.OnPersonCreated(new Events.Args.PersonCreatedEventArgs
            {
                CreatedBy = User,
                Person = person
            }, this);

            return CreatedAtAction(nameof(Get), new {id = person.Id}, new DTOs.Person.Get(User, person));
        }

        /// <summary>
        /// Modifes a person.
        /// </summary>
        /// <param name="id">The id of the person to modify.</param>
        /// <param name="dto">A dto containing all the information needed to modify a person.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DTOs.Person.Get), 201)]
        public IActionResult Put(Guid id, [FromBody] DTOs.Person.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!TryGet(id, out Person person))
                return NotFoundParameter(id, nameof(id));

            if (!TryGet(dto.Ethnicity, out Ethnicity ethnicity))
                return NotFoundParameter(dto.Ethnicity, nameof(dto.Ethnicity));

            if (!TryGet(dto.ReligiousPreference, out ReligiousPreference religiousPreference))
                return NotFoundParameter(dto.ReligiousPreference, nameof(dto.ReligiousPreference));

            if (!TryGet(dto.Designation, out Designation designation))
                return NotFoundParameter(dto.Designation, nameof(dto.Designation));

            if (!TryGet(dto.Division, out Division division))
                return NotFoundParameter(dto.Division, nameof(dto.Division));

            if (!TryGet(dto.UIC, out UIC uic))
                return NotFoundParameter(dto.UIC, nameof(dto.UIC));

            person.BilletAssignment = dto.BilletAssignment;
            person.DateOfArrival = dto.DateOfArrival;
            person.DateOfBirth = dto.DateOfBirth;
            person.DateOfDeparture = dto.DateOfDeparture;
            person.Designation = designation;
            person.Division = division;
            person.DoDId = dto.DoDId;
            person.DutyStatus = dto.DutyStatus;
            person.EAOS = dto.EAOS;
            person.Ethnicity = ethnicity;
            person.FirstName = dto.FirstName;
            person.JobTitle = dto.JobTitle;
            person.LastName = dto.LastName;
            person.MiddleName = dto.MiddleName;
            person.Paygrade = dto.Paygrade;
            person.PRD = dto.PRD;
            person.ReligiousPreference = religiousPreference;
            person.Sex = dto.Sex;
            person.Shift = dto.Shift;
            person.Suffix = dto.Suffix;
            person.Supervisor = dto.Supervisor;
            person.UIC = uic;
            person.WorkCenter = dto.WorkCenter;
            person.WorkRoom = dto.WorkRoom;

            var results = person.Validate();
            if (!results.IsValid)
                return BadRequestWithValidationErrors(results);

            var failedProperties = new List<string>();
            foreach (var change in GetEntityChanges(person))
            {
                if (!User.CanEdit(person, change.PropertyPath))
                    failedProperties.Add(change.PropertyPath);

                Save(change);
            }

            if (failedProperties.Any())
                return Forbid(
                    $"You were not allowed to edit the following properties: {String.Join(", ", failedProperties)}");

            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = person.Id}, new DTOs.Person.Get(User, person));
        }
    }
}