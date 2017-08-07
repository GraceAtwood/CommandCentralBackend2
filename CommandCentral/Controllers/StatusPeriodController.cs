using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities.Muster;
using NHibernate.Criterion;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Enums;
using CommandCentral.Utilities.Types;

namespace CommandCentral.Controllers
{
    /// <summary>
    /// <para>A status period is used to indicate that a person will be in a status other than "Present" for a given period of time.</para>
    /// <para>Status periods are used to inform the muster each day by pre-setting a person's muster status and to indicate that person is unavailable for watch (by setting the ExemptsFromWatch field).</para>
    /// <para>Clients in a person's chain of command may submit status periods; however, if a status period is said to exempt a person from watch, then membership in a person's watchbill chain of command is also required.</para>
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class StatusPeriodController : CommandCentralController
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
        /// <param name="person">The id of the person for whom a status period was submitted.</param>
        /// <param name="submittedBy">The id of the person who submitted a status period.</param>
        /// <param name="start">Defines the starting date and time of a window in which to search for any status period that overlaps with that window.  If left blank, the search window is assumed to start at the beginning of time.</param>
        /// <param name="end">Defines the ending date and time of a window in which to search for any status period that overlaps with that window.  If left blank, the search window is assumed to end at the end of time.</param>
        /// <param name="reason">The id of the reason to search for.</param>
        /// <param name="exemptsFromWatch">true/false</param>
        /// <param name="limit">[Default = 1000] Indicates that the api should return no more than this number of records.  Does not guarantee that the api will return at least this many records even if there are more than this number in the database due to after-load authorization checks.</param>
        /// <param name="orderBy">[Default = start][Valid values = start, datesubmitted] Instructs the api to order the results by this field (this also affects which records are returned if limit is given).</param>
        /// <returns></returns>
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.StatusPeriod.Get>))]
        public IActionResult Get([FromQuery] Guid? person, [FromQuery] Guid? submittedBy, [FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] Guid? reason, [FromQuery] bool? exemptsFromWatch, [FromQuery] int limit = 1000, [FromQuery] string orderBy = nameof(TimeRange.Start))
        {
            if (limit <= 0)
                return BadRequest($"The value '{limit}' for the property '{nameof(limit)}' was invalid.  It must be greater than zero.");

            var query = DBSession.QueryOver<StatusPeriod>();

            if (person.HasValue)
                query = query.Where(x => x.Person.Id == person);

            if (submittedBy.HasValue)
                query = query.Where(x => x.SubmittedBy.Id == submittedBy);

            if (start.HasValue && !end.HasValue)
                query = query.Where(x => x.Range.Start >= start || x.Range.End >= start);
            else if (end.HasValue && !start.HasValue)
                query = query.Where(x => x.Range.Start <= end || x.Range.End <= end);
            else if (end.HasValue && end.HasValue)
                query = query.Where(x => x.Range.Start <= end && x.Range.End >= start);

            if (reason.HasValue)
                query = query.Where(x => x.AccountabilityType.Id == reason);

            if (exemptsFromWatch.HasValue)
                query = query.Where(x => x.ExemptsFromWatch == exemptsFromWatch);

            if (String.Equals(orderBy, nameof(TimeRange.Start), StringComparison.CurrentCultureIgnoreCase))
                query = query.OrderBy(x => x.Range.Start).Desc;
            else if (String.Equals(orderBy, nameof(StatusPeriod.DateSubmitted), StringComparison.CurrentCultureIgnoreCase))
                query = query.OrderBy(x => x.DateSubmitted).Desc;
            else
                return BadRequest($"Your requested value '{orderBy}' for the parameter '{nameof(orderBy)}' is not supported.  The supported values are '{nameof(TimeRange.Start)}' (this is the default) and '{nameof(StatusPeriod.DateSubmitted)}'.");

            var result = query.OrderBy(x => x.Range.Start).Desc.Take(limit)
                .Future()
                .Where(statusPeriod => User.GetFieldPermissions<Person>(statusPeriod.Person).CanReturn(x => x.StatusPeriods))
                .Select(statusPeriod =>
                    new DTOs.StatusPeriod.Get
                    {
                        DateSubmitted = statusPeriod.DateSubmitted,
                        ExemptsFromWatch = statusPeriod.ExemptsFromWatch,
                        Id = statusPeriod.Id,
                        Person = statusPeriod.Person.Id,
                        Range = statusPeriod.Range,
                        Reason = statusPeriod.AccountabilityType.Id,
                        SubmittedBy = statusPeriod.SubmittedBy.Id,
                        DateLastModified = statusPeriod.DateLastModified,
                        LastModifiedBy = statusPeriod.LastModifiedBy.Id
                    });

            return Ok(result.ToList());
        }

        /// <summary>
        /// Returns the status period identified by the given Id.
        /// </summary>
        /// <param name="id">The id of the status period to return.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.StatusPeriod.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<StatusPeriod>(id);
            if (item == null)
                return NotFound();

            if (!User.GetFieldPermissions<Person>(item.Person).CanReturn(x => x.StatusPeriods))
                return Forbid();

            return Ok(new DTOs.StatusPeriod.Get
            {
                DateSubmitted = item.DateSubmitted,
                ExemptsFromWatch = item.ExemptsFromWatch,
                Id = item.Id,
                Person = item.Person.Id,
                Range = item.Range,
                Reason = item.AccountabilityType.Id,
                SubmittedBy = item.SubmittedBy.Id,
                DateLastModified = item.DateLastModified,
                LastModifiedBy = item.LastModifiedBy.Id
            });
        }

        /// <summary>
        /// Creates a new status period.
        /// </summary>
        /// <param name="dto">The dto containing the data required to create the new status period.</param>
        /// <returns></returns>
        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.StatusPeriod.Get))]
        public IActionResult Post([FromBody]DTOs.StatusPeriod.Post dto)
        {
            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFound($"Unable to find object referenced by parameter: {nameof(dto.Person)}.");

            if (dto.ExemptsFromWatch && !User.IsInChainOfCommand(person, ChainsOfCommand.QuarterdeckWatchbill))
            {
                return Forbid("Must be in the Watchbill chain of command to exempt a person from watch.");
            }

            if (!User.GetFieldPermissions<Person>(person).CanEdit(x => x.StatusPeriods))
                return Forbid("Can not submit a status period for this person.");

            var reason = ReferenceListHelper<AccountabilityType>.Get(dto.Reason);
            if (reason == null)
                return NotFound($"Unable to find object referenced by parameter: {nameof(dto.Reason)}.");

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

                return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.StatusPeriod.Get
                {
                    DateSubmitted = item.DateSubmitted,
                    ExemptsFromWatch = item.ExemptsFromWatch,
                    Id = item.Id,
                    Person = item.Person.Id,
                    Range = item.Range,
                    Reason = item.AccountabilityType.Id,
                    SubmittedBy = item.SubmittedBy.Id,
                    DateLastModified = item.DateLastModified,
                    LastModifiedBy = item.LastModifiedBy.Id
                });
            }
        }

        /// <summary>
        /// Updates the given status period.
        /// </summary>
        /// <param name="id">The id of the status period to update.</param>
        /// <param name="dto">The dto containing all of the data to update.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.StatusPeriod.Get))]
        public IActionResult Put(Guid id, [FromBody]DTOs.StatusPeriod.Put dto)
        {
            var item = DBSession.Get<StatusPeriod>(id);
            if (item == null)
                return NotFound();

            if (item.ExemptsFromWatch && !User.IsInChainOfCommand(item.Person, ChainsOfCommand.QuarterdeckWatchbill))
            {
                return Forbid("Must be in the Watchbill chain of command to modify a status period that exempts a person from watch.");
            }

            if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.StatusPeriods))
            {
                return Forbid();
            }

            var reason = ReferenceListHelper<AccountabilityType>.Get(dto.Reason);
            if (reason == null)
                return NotFound($"Unable to find object referenced by parameter: {nameof(dto.Reason)}.");

            using (var transaction = DBSession.BeginTransaction())
            {
                item.ExemptsFromWatch = dto.ExemptsFromWatch;
                item.Range = dto.Range;
                item.AccountabilityType = reason;
                item.LastModifiedBy = User;
                item.DateLastModified = CallTime;

                var result = item.Validate();
                if (!result.IsValid)
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));

                DBSession.Update(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.StatusPeriod.Get
                {
                    DateSubmitted = item.DateSubmitted,
                    ExemptsFromWatch = item.ExemptsFromWatch,
                    Id = item.Id,
                    Person = item.Person.Id,
                    Range = item.Range,
                    Reason = item.AccountabilityType.Id,
                    SubmittedBy = item.SubmittedBy.Id,
                    DateLastModified = item.DateLastModified,
                    LastModifiedBy = item.LastModifiedBy.Id
                });
            }
        }

        /// <summary>
        /// Deletes the given status period.
        /// </summary>
        /// <param name="id">The id of the status period to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200)]
        public IActionResult Delete(Guid id)
        {
            var item = DBSession.Get<StatusPeriod>(id);
            if (item == null)
                return NotFound();

            if (item.ExemptsFromWatch && !User.IsInChainOfCommand(item.Person, ChainsOfCommand.QuarterdeckWatchbill))
            {
                return Forbid("Must be in the Watchbill chain of command to delete a status period that exempts a person from watch.");
            }

            if (!User.GetFieldPermissions<Person>(item.Person).CanEdit(x => x.StatusPeriods))
            {
                return Forbid();
            }

            if (item.Range.Start <= DateTime.UtcNow)
                return Conflict("You may not delete a status period whose time range has already started.  You may only modify its ending time.");

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Delete(item);

                transaction.Commit();
            }

            return Ok();
        }
    }
}
