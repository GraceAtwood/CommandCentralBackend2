using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities.Muster;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Enums;
using CommandCentral.Utilities.Types;
using NHibernate.Linq;
using System.Linq.Expressions;
using CommandCentral.Framework.Data;
using LinqKit;

namespace CommandCentral.Controllers
{
    /// <summary>
    /// <para>A status period is used to indicate that a person will be in a status other than "Present" for a given period of time.</para>
    /// <para>Status periods are used to inform the muster each day by pre-setting a person's muster status and to indicate that person is unavailable for watch (by setting the ExemptsFromWatch field).</para>
    /// <para>Clients in a person's chain of command may submit status periods; however, if a status period is said to exempt a person from watch, then membership in a person's watchbill chain of command is also required.</para>
    /// </summary>
    public class StatusPeriodsController : CommandCentralController
    {
        /// <summary>
        /// Queries against status periods.
        /// 
        /// Status periods are only returned if the client has permission to view the StatusPeriod.Person's status periods.
        /// 
        /// This qualification is taken into account after the database load, so it is possible that a limit of 1000 records could potentially return less as records are filtered out.
        /// 
        /// For this reason, limit should be seen as "return no more than this number of records".
        /// </summary>
        /// <param name="person">The person for whom a status period was submitted.  Supports either Id selection or simple search-based query combined with a conjunction.</param>
        /// <param name="submittedBy">The person who submitted a status period.  Supports either Id selection or simple search-based query combined with a conjunction.</param>
        /// <param name="range">Defines a time range query for the range of status periods.</param>
        /// <param name="accountabilityType">The accountability type or code to search for.  Supports either Id selection or simple search-based query combined with a disjunction.</param>
        /// <param name="exemptsFromWatch">true/false</param>
        /// <param name="limit">[Default = 1000] Indicates that the api should return no more than this number of records.  Does not guarantee that the api will return at least this many records even if there are more than this number in the database due to after-load authorization checks.</param>
        /// <param name="orderBy">[Default = start][Valid values = start, datesubmitted] Instructs the api to order the results by this field (this also affects which records are returned if limit is given).</param>
        /// <returns></returns>
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.StatusPeriod.Get>))]
        public IActionResult Get([FromQuery] string person, [FromQuery] string submittedBy, [FromQuery] DTOs.DateTimeRangeQuery range, 
            [FromQuery] string accountabilityType, [FromQuery] bool? exemptsFromWatch, [FromQuery] int limit = 1000, [FromQuery] string orderBy = nameof(TimeRange.Start))
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var predicate = ((Expression<Func<StatusPeriod, bool>>) null)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddPersonQueryExpression(x => x.SubmittedBy, submittedBy)
                .AddReferenceListQueryExpression(x => x.AccountabilityType, accountabilityType)
                .AddTimeRangeQueryExpression(x => x.Range, range)
                .AddNullableBoolQueryExpression(x => x.ExemptsFromWatch, exemptsFromWatch);

            var query = DBSession.Query<StatusPeriod>()
                .AsExpandable()
                .NullSafeWhere(predicate);

            if (String.Equals(orderBy, nameof(TimeRange.Start), StringComparison.CurrentCultureIgnoreCase))
                query = query.OrderByDescending(x => x.Range.Start);
            else if (String.Equals(orderBy, nameof(StatusPeriod.DateSubmitted), StringComparison.CurrentCultureIgnoreCase))
                query = query.OrderByDescending(x => x.DateSubmitted);
            else
                return BadRequest($"Your requested value '{orderBy}' for the parameter '{nameof(orderBy)}' is not supported.  The supported values are '{nameof(TimeRange.Start)}' (this is the default) and '{nameof(StatusPeriod.DateSubmitted)}'.");

            var result = query
                .Take(limit)
                .ToList()
                .Where(statusPeriod => User.GetFieldPermissions<Person>(statusPeriod.Person).CanReturn(x => x.StatusPeriods))
                .Select(item => new DTOs.StatusPeriod.Get(item));

            return Ok(result.ToList());
        }

        /// <summary>
        /// Retrieves the status period identified by the given Id.
        /// </summary>
        /// <param name="id">The id of the status period to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.StatusPeriod.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<StatusPeriod>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.GetFieldPermissions<Person>(item.Person).CanReturn(x => x.StatusPeriods))
                return Forbid();

            return Ok(new DTOs.StatusPeriod.Get(item));
        }

        /// <summary>
        /// Creates a new status period.
        /// </summary>
        /// <param name="dto">The dto containing the data required to create the new status period.</param>
        /// <returns></returns>
        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.StatusPeriod.Get))]
        public IActionResult Post([FromBody]DTOs.StatusPeriod.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFoundParameter(dto.Person, nameof(dto.Person));

            if (dto.ExemptsFromWatch && !User.IsInChainOfCommand(person, ChainsOfCommand.QuarterdeckWatchbill))
                return Forbid("Must be in the Watchbill chain of command to exempt a person from watch.");

            if (!User.GetFieldPermissions<Person>(person).CanEdit(x => x.StatusPeriods))
                return Forbid("Can not submit a status period for this person.");

            var reason = DBSession.Get<AccountabilityType>(dto.Reason);
            if (reason == null)
                return NotFoundParameter(dto.Reason, nameof(dto.Reason));

            var item = new StatusPeriod
            {
                DateSubmitted = CallTime,
                ExemptsFromWatch = dto.ExemptsFromWatch,
                Id = Guid.NewGuid(),
                Person = person,
                Range = dto.Range,
                AccountabilityType = reason,
                SubmittedBy = User,
                DateLastModified = CallTime,
                LastModifiedBy = User
            };

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.StatusPeriod.Get(item));
        }

        /// <summary>
        /// Updates the given status period.
        /// </summary>
        /// <param name="id">The id of the status period to update.</param>
        /// <param name="dto">The dto containing all of the data to update.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.StatusPeriod.Get))]
        public IActionResult Put(Guid id, [FromBody]DTOs.StatusPeriod.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var item = DBSession.Get<StatusPeriod>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (item.ExemptsFromWatch && !User.IsInChainOfCommand(item.Person, ChainsOfCommand.QuarterdeckWatchbill))
                return Forbid("Must be in the Watchbill chain of command to modify a status period that exempts a person from watch.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.StatusPeriods))
                return Forbid();

            var reason = DBSession.Get<AccountabilityType>(dto.Reason);
            if (reason == null)
                return NotFoundParameter(dto.Reason, nameof(dto.Reason));

            item.ExemptsFromWatch = dto.ExemptsFromWatch;
            item.Range = dto.Range;
            item.AccountabilityType = reason;
            item.LastModifiedBy = User;
            item.DateLastModified = CallTime;

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Update(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.StatusPeriod.Get(item));
        }

        /// <summary>
        /// Deletes the given status period.
        /// </summary>
        /// <param name="id">The id of the status period to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var item = DBSession.Get<StatusPeriod>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (item.ExemptsFromWatch && !User.IsInChainOfCommand(item.Person, ChainsOfCommand.QuarterdeckWatchbill))
                return Forbid("Must be in the Watchbill chain of command to delete a status period that exempts a person from watch.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.StatusPeriods))
                return Forbid();

            if (item.Range.Start <= DateTime.UtcNow)
                return Conflict("You may not delete a status period whose time range has already started.  You may only modify its ending time.");

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Delete(item);
                transaction.Commit();
            }

            return NoContent();
        }
    }
}
