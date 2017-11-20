using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.Muster;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Enums;
using CommandCentral.Events;
using CommandCentral.Events.Args;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;

namespace CommandCentral.Controllers.WatchbillControllers
{
    /// <summary>
    /// Provides access to the watchbills resource.  Permissions are not required to GET watchbills, 
    /// but command level watchbill permissions are required to modify the resource.
    /// </summary>
    public class WatchbillsController : CommandCentralController
    {
        /// <summary>
        /// Performs a query against the watchbills.
        /// </summary>
        /// <param name="title">A string query for the title of a watchbill.</param>
        /// <param name="month">An integer query for the month of the watchbill.  Multiple integers may be joined by the OR query parameter.</param>
        /// <param name="year">An integer query for the month of the watchbill.  Multiple integers may be joined by the OR query parameter.</param>
        /// <param name="command">A command query for the watchbill's command.</param>
        /// <param name="phase">An exact enum query for the phase of the watchbill.</param>
        /// <param name="createdBy">A person query for the person who created a watchbill.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Watchbill.Get>))]
        public IActionResult Get([FromQuery] string title, [FromQuery] string month, [FromQuery] string year,
            [FromQuery] string command, [FromQuery] string phase, [FromQuery] string createdBy)
        {
            var predicate = ((Expression<Func<Watchbill, bool>>) null)
                .AddCommandQueryExpression(x => x.Command, command)
                .AddPersonQueryExpression(x => x.CreatedBy, createdBy)
                .AddStringQueryExpression(x => x.Title, title)
                .AddExactEnumQueryExpression(x => x.Phase, phase)
                .AddIntQueryExpression(x => x.Month, month)
                .AddIntQueryExpression(x => x.Year, year);

            var results = DBSession.Query<Watchbill>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToList()
                .Select(x => new DTOs.Watchbill.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Retrieves the watchbill identified by the given Id.
        /// </summary>
        /// <param name="id">The id of the watchbill to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.Watchbill.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<Watchbill>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.Watchbill.Get(item));
        }

        /// <summary>
        /// Creates a new watchbill.  Client must be in the watchbill chain of command.  Only one watchbill may be 
        /// created for a given command, month, and year combination.
        /// </summary>
        /// <param name="dto">A dto containing all of the required information to create a watchbill.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.Watchbill.Get))]
        public IActionResult Post([FromBody] DTOs.Watchbill.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill] !=
                ChainOfCommandLevels.Command)
                return Forbid("You must be in the command level of the watchbill chain of command.");

            var command = DBSession.Get<Command>(dto.Command);
            if (command == null)
                return NotFoundParameter(dto.Command, nameof(dto.Command));

            if (DBSession.Query<Watchbill>()
                    .Count(x => x.Command == command && x.Month == dto.Month && x.Year == dto.Year) != 0)
                return Conflict("A watchbill already exists for the given command, month, and year combination.  " +
                                "Please considering using that one or deleting it.");

            var watchbill = new Watchbill
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Year = dto.Year,
                Month = dto.Month,
                Command = command,
                Phase = WatchbillPhases.Initial,
                CreatedBy = User
            };

            var result = watchbill.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(watchbill);
            CommitChanges();

            return CreatedAtAction(nameof(Get),
                new {id = watchbill.Id},
                new DTOs.Watchbill.Get(watchbill));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.Watchbill.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.Watchbill.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill] !=
                ChainOfCommandLevels.Command)
                return Forbid("You must be in the command level of the watchbill chain of command.");

            var watchbill = DBSession.Get<Watchbill>(id);
            if (watchbill == null)
                return NotFoundParameter(id, nameof(id));

            var phaseWasUpdated = false;
            if (watchbill.Phase != dto.Phase)
            {
                if (dto.Phase < watchbill.Phase)
                    return BadRequest("You may not update my a watchbill by moving it back in the phases.  " +
                                      $"Requested phase: {dto.Phase}.  Current phase: {watchbill.Phase}");

                if (dto.Phase != watchbill.Phase + 1)
                    return BadRequest("You may only update the phase by moving it forward one phase at a time.  " +
                                      $"Requested phase: {dto.Phase}.  Current phase: {watchbill.Phase}");

                phaseWasUpdated = true;
                watchbill.Phase = dto.Phase;
            }
            watchbill.Title = dto.Title;

            var result = watchbill.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            CommitChanges();

            if (phaseWasUpdated)
                Task.Run(() => HandleWatchbillPhaseUpdate(watchbill.Id));

            return CreatedAtAction(nameof(Get), new {id = watchbill.Id}, new DTOs.Watchbill.Get(watchbill));
        }

        /// <summary>
        /// The caller is responsible for ensuring that the watchbill phase update was valid.
        /// This method only alerts the required parties of the current phase of the watchbill and takes any actions 
        /// that phase might require.
        /// NOTE: This method is vulnerable to a race condition that could occur if a client attempted to update 
        /// a watchbill quickly through the phases.  The only way I can think of to fix this is to implement row-level 
        /// locking at the database which I think you can do through FluentNHibernate.  Regardless, I'm not going down 
        /// that hole right now.  I'm sorry to the future maintainer who finds this message due to a race condition.
        /// </summary>
        /// <param name="watchbillId">The id of the watchbill for which to alert users of changes.</param>
        private static void HandleWatchbillPhaseUpdate(Guid watchbillId)
        {
            using (var session = SessionManager.GetCurrentSession())
            using (var transaction = session.BeginTransaction())
            {
                var watchbill = session.Get<Watchbill>(watchbillId);
                if (watchbill == null)
                    throw new ArgumentException("The  given watchbill id did not refer to an actual watchbill.",
                        nameof(watchbillId));

                switch (watchbill.Phase)
                {
                    case WatchbillPhases.Initial:
                    {
                        //Right now, we don't do anything when the watchbill is created.
                        break;
                    }
                    case WatchbillPhases.Assignment:
                    {
                        AssignShiftsToDivisions(watchbill, session);
                        EventManager.OnWatchbillAssigned(new WatchbillEventArgs { Watchbill = watchbill }, null);
                        break;
                    }
                    case WatchbillPhases.Review:
                    {
                        break;
                    }
                    case WatchbillPhases.Publish:
                    {
                        break;
                    }
                    default:
                        throw new Exception("An unknown phase fell to the default case in the " +
                                            $"switch in the method {nameof(HandleWatchbillPhaseUpdate)}");
                }

                transaction.Commit();
            }
        }

        private static void AssignShiftsToDivisions(Watchbill watchbill, ISession session)
        {
            //First, we're going to need all the status period inputs for the watchbill's time range.
            var statusPeriodsByDivision = session.Query<StatusPeriod>()
                .Where(x => x.ExemptsFromWatch &&
                            x.Person.Division.Department.Command == watchbill.Command &&
                            x.Person.DutyStatus == DutyStatuses.Active &&
                            x.Range.Start >= watchbill.GetFirstDay() && x.Range.End <= watchbill.GetLastDay())
                .ToFuture()
                .GroupBy(x => x.Person.Division);

            var divisionByTotalAvailableMinutes = new Dictionary<Division, double>();
            var totalAvailableMinutes = 0D;

            foreach (var divisionGroup in statusPeriodsByDivision)
            {
                var total = divisionGroup.Sum(statusPeriod => statusPeriod.Range.GetTotalMinutes());
                divisionByTotalAvailableMinutes[divisionGroup.Key] = total;
                totalAvailableMinutes += total;
            }

            var shiftsToAssignEachDivision = divisionByTotalAvailableMinutes
                .ToDictionary(x => x.Key, x => (x.Value / totalAvailableMinutes) * watchbill.WatchShifts.Count)
                .OrderByDescending(x => x.Value - Math.Truncate(x.Value))
                .ToList();

            var startIndex = 0;
            var shifts = watchbill.WatchShifts.Shuffle();
            
            foreach (var pair in shiftsToAssignEachDivision)
            {
                for (var x = startIndex; x < startIndex + (int) pair.Value; x++)
                {
                    shifts[x].DivisionAssignedTo = pair.Key;
                }
                
                startIndex += (int) pair.Value;
            }

            for (var x = 0; x < shifts.Count - startIndex; x++)
            {
                shifts[shifts.Count - 1 - x].DivisionAssignedTo = shiftsToAssignEachDivision[x].Key;
            }
        }

        /// <summary>
        /// Deletes the identified watchbill.
        /// </summary>
        /// <param name="id">The id of the watchbill to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill] !=
                ChainOfCommandLevels.Command)
                return Forbid("You must be in the command level of the watchbill chain of command.");

            var watchbill = DBSession.Get<Watchbill>(id);
            if (watchbill == null)
                return NotFoundParameter(id, nameof(id));

            var tempDate = new DateTime(watchbill.Year, watchbill.Month, 1);
            if (tempDate < DateTime.UtcNow)
                return BadRequest("You may not delete a watchbill whose first day has already passed.");

            DBSession.Delete(watchbill);
            CommitChanges();

            return NoContent();
        }
    }
}