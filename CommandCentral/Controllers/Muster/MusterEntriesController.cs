using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities.Muster;
using CommandCentral.Authorization;
using CommandCentral.Enums;
using NHibernate.Linq;
using System.Linq.Expressions;
using CommandCentral.Utilities;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Entities;
using CommandCentral.Utilities.Types;
using CommandCentral.Framework.Data;
using LinqKit;

namespace CommandCentral.Controllers
{
    /// <summary>
    /// A muster entry represents a single accounting of a person during a muster cycle.
    /// Anyone can see all muster entries; however, modification of muster entries requires that you be in the given person's muster chain of command.
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class MusterEntriesController : CommandCentralController
    {
        /// <summary>
        /// Queries muster entries.
        /// </summary>
        /// <param name="person">The person for whom a muster entry was submitted.  Supports either Id selection or simple search-based query combined with a conjunction.</param>
        /// <param name="submittedBy">The person who submitted a muster entry.  Supports either Id selection or simple search-based query combined with a conjunction.</param>
        /// <param name="range">A time range query for the parent cycle's time range.</param>
        /// <param name="accountabilityType">The accountability type or code to search for.  Supports either Id selection or simple search-based query combined with a disjunction.</param>
        /// <param name="musterCycle">The Id of the muster cycle to which your desired muster entries are connected.</param>
        /// <param name="statusPeriodSetBy">The id of the status period that set the muster entries you are interested in.</param>
        /// <param name="setByStatusPeriod">true/false if a muster entry was set by a status period.</param>
        /// <param name="limit">[Default = 1000] Indicates that the api should return no more than this number of records.</param>
        /// <param name="orderBy">[Default = start][Valid values = start, datesubmitted] Instructs the api to order the results by this field (this also affects which records are returned if limit is given).</param>
        /// <returns></returns>
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.MusterEntry.Get>))]
        public IActionResult Get([FromQuery] string person, [FromQuery] string submittedBy, [FromQuery] DTOs.DateTimeRangeQuery range, [FromQuery] string accountabilityType,
            [FromQuery] Guid? musterCycle, [FromQuery] Guid? statusPeriodSetBy, [FromQuery] bool? setByStatusPeriod, [FromQuery] int limit = 1000, [FromQuery] string orderBy = nameof(TimeRange.Start))
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            Expression<Func<MusterEntry, bool>> predicate = null;

            predicate = predicate
                .AddPersonQueryExpression(x => x.Person, person)
                .AddPersonQueryExpression(x => x.SubmittedBy, submittedBy)
                .AddTimeRangeQueryExpression(x => x.MusterCycle.Range, range)
                .AddReferenceListQueryExpression(x => x.AccountabilityType, accountabilityType);

            if (musterCycle.HasValue)
                predicate = predicate.NullSafeAnd(x => x.MusterCycle.Id == musterCycle);

            if (statusPeriodSetBy.HasValue)
                predicate = predicate.NullSafeAnd(x => x.StatusPeriodSetBy.Id == statusPeriodSetBy);

            if (setByStatusPeriod.HasValue)
                predicate = predicate.NullSafeAnd(x => x.StatusPeriodSetBy != null);

            var query = DBSession.Query<MusterEntry>()
                .AsExpandable()
                .NullSafeWhere(predicate);

            if (String.Equals(orderBy, nameof(TimeRange.Start), StringComparison.CurrentCultureIgnoreCase))
                query = query.OrderByDescending(x => x.MusterCycle.Range.Start);
            else if (String.Equals(orderBy, nameof(MusterEntry.TimeSubmitted), StringComparison.CurrentCultureIgnoreCase))
                query = query.OrderByDescending(x => x.TimeSubmitted);
            else
                return BadRequest("That order by parameter is not supported.");

            var results = query
                .Take(limit)
                .ToFuture()
                .Select(x => new DTOs.MusterEntry.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Retrieves all muster entries tied to the muster cycle that is tied to the client's command.
        /// </summary>
        /// <returns></returns>
        [HttpGet("current")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.MusterEntry.Get))]
        public IActionResult GetCurrent()
        {
            var entries = User.Command.CurrentMusterCycle.MusterEntries
                .Select(entry => new DTOs.MusterEntry.Get(entry))
                .ToList();

            return Ok(entries);
        }

        /// <summary>
        /// Retrieves the muster entry identified by the given Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.MusterEntry.Get))]
        public IActionResult Get(Guid id)
        {
            var entry = DBSession.Get<MusterEntry>(id);
            if (entry == null)
                return NotFound();

            return Ok(new DTOs.MusterEntry.Get(entry));
        }

        /// <summary>
        /// Creates a new muster entry given a person and an accountability type.  The muster entry will automatically be submitted to the current muster cycle for the client's command.  
        /// Clients may not submit muster entries to other commands' muster cycles nor may they submit muster entries to any muster cycle but the current one.  
        /// If a muster entry already exists within that muster cycle for the given person, you will receive a 409 CONFLICT response with the muster entry you are in conflict with in the body of the response.
        /// </summary>
        /// <param name="dto">The object containing the data necessary to create a new muster entry.</param>
        /// <returns></returns>
        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.MusterEntry.Get))]
        public IActionResult Post([FromBody]DTOs.MusterEntry.Post dto)
        {
            if (dto == null)
                return BadRequest();

            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFound($"The object referenced by your parameter '{nameof(dto.Person)}' could not be found.");

            if (!User.IsInChainOfCommand(person, ChainsOfCommand.Muster))
                return Forbid();

            var accountabilityType = DBSession.Get<AccountabilityType>(dto.AccountabilityType);
            if (accountabilityType == null)
                return NotFound($"The object referenced by your parameter '{nameof(dto.AccountabilityType)}' could not be found.");

            var musterCycle = person.Command.CurrentMusterCycle;

            var existingEntry = DBSession.Query<MusterEntry>().FirstOrDefault(x => x.MusterCycle == musterCycle && x.Person == person);
            if (existingEntry != null)
                return Conflict(new DTOs.MusterEntry.Get(existingEntry));

            var entry = new MusterEntry
            {
                AccountabilityType = accountabilityType,
                Id = Guid.NewGuid(),
                MusterCycle = musterCycle,
                Person = person,
                SubmittedBy = User,
                TimeSubmitted = CallTime
            };

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(entry);
                transaction.Commit();
            }

            Events.EventManager.OnMusterEntrySubmitted(new Events.Args.MusterEntryEventArgs
            {
                MusterEntry = entry
            }, this);

            return CreatedAtAction(nameof(Get), new { id = entry.Id }, new DTOs.MusterEntry.Get(entry));
        }

        /// <summary>
        /// Patches the muster entry identified by the given Id.  Only the accountability type may be changed.  This endpoint will also reset the time submitted and submitted by properties to the call time and the client.
        /// </summary>
        /// <param name="id">The id of the muster entry to patch.</param>
        /// <param name="dto">The dto containing all of the information necessary to patch the muster entry.</param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.MusterEntry.Get))]
        public IActionResult Patch(Guid id, [FromBody]DTOs.MusterEntry.Patch dto)
        {
            if (dto == null)
                return BadRequest();

            var entry = DBSession.Get<MusterEntry>(id);
            if (entry == null)
                return NotFound();

            if (!User.IsInChainOfCommand(entry.Person, ChainsOfCommand.Muster))
                return Forbid();

            var accountabilityType = DBSession.Get<AccountabilityType>(dto.AccountabilityType);
            if (accountabilityType == null)
                return NotFound($"The object referenced by your parameter '{nameof(dto.AccountabilityType)}' could not be found.");

            using (var transaction = DBSession.BeginTransaction())
            {
                entry.AccountabilityType = accountabilityType;
                entry.TimeSubmitted = CallTime;
                entry.SubmittedBy = User;

                DBSession.Update(entry);
                transaction.Commit();
            }

            return Ok(new DTOs.MusterEntry.Get(entry));
        }

        /// <summary>
        /// Deletes the muster entry identified by the given Id.
        /// </summary>
        /// <param name="id">The id of the muster entry to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200)]
        public IActionResult Delete(Guid id)
        {
            var entry = DBSession.Get<MusterEntry>(id);
            if (entry == null)
                return NotFound();

            if (!User.IsInChainOfCommand(entry.Person, ChainsOfCommand.Muster))
                return Forbid();

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Delete(entry);

                Events.EventManager.OnMusterEntryDeleted(new Events.Args.MusterEntryEventArgs
                {
                    MusterEntry = entry
                }, this);

                transaction.Commit();
            }

            return NoContent();
        }
    }
}
