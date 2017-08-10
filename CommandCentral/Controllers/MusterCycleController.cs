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

namespace CommandCentral.Controllers
{
    /// <summary>
    /// Muster cycles are used to encapsulate all of the muster entries for a given period of time.  Essentially, this is a single day's muster.
    /// <para/>
    /// The starting time of a muster cycle is determined by the command that owns it, and specifically the muster start hour property on the owning command.
    /// <para/>
    /// If you are looking for "today's muster" for your client, simply use the GET /mustercycle/current endpoint.  This will get the muster cycle associated with your client's command.
    /// <para/> 
    /// The only thing consumers may modify about a muster cycle is the isFinalized property (requires access to the admin tools submodule).  
    /// This is how you close or reopen the muster cycle.  The rest of the muster cycle's life cycle is handled automatically.
    /// </summary>
    [Route("api/[controller]")]
    public class MusterCycleController : CommandCentralController
    {
        /// <summary>
        /// Queries against the muster cycles.
        /// 
        /// A muster cycle that has isFinalized=false but has FinalizedBy set to a person is a muster cycle that was reopened.  In this instance, FinalizedBy will contain the last person to finalize it, not the person that reopened it.
        /// </summary>
        /// <param name="from">Defines the starting date and time of a window in which to search for any muster cycle that overlaps with that window.  If left blank, the search window is assumed to start at the beginning of time.</param>
        /// <param name="to">Defines the ending date and time of a window in which to search for any muster cycle that overlaps with that window.  If left blank, the search window is assumed to end at the end of time.</param>
        /// <param name="isFinalized">true/false</param>
        /// <param name="finalizedBy">
        /// The person who finalized this muster cycle.  Supports either Id selection or simple search-based query combined with a conjunction.  
        /// If looking for cycles that haven't been finalized yet, please use the isFinalized field.  
        /// You can use the magic string "[system]" to find muster cycles that were automatically finalized by the system at their rollover times.  Using "[system]" will override any given isFinalized parameter.</param>
        /// <param name="command">The command to which a muster cycle belongs.  Supports either Id selection or simple search-based query combined with a disjunction.</param>
        /// <param name="limit">[Default = 1000] Indicates that the api should return no more than this number of records.</param>
        /// <returns></returns>
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.MusterCycle.Get>))]
        public IActionResult Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] bool? isFinalized, [FromQuery] string finalizedBy, [FromQuery] string command, [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequest($"The value '{limit}' for the property '{nameof(limit)}' was invalid.  It must be greater than zero.");

            var query = DBSession.Query<MusterCycle>();

            if (!String.IsNullOrWhiteSpace(finalizedBy))
            {
                if (finalizedBy.Equals("[system]", StringComparison.CurrentCultureIgnoreCase))
                {
                    query = query.Where(x => x.FinalizedBy == null && x.IsFinalized == true);
                }
                else if (Guid.TryParse(finalizedBy, out Guid id))
                {
                    query = query.Where(x => x.FinalizedBy.Id == id);
                }
                else
                {
                    foreach (var term in finalizedBy.Split(',', ' ', ';', '-'))
                    {
                        query = query.Where(x =>
                            x.FinalizedBy.FirstName.Contains(term) ||
                            x.FinalizedBy.LastName.Contains(term) ||
                            x.FinalizedBy.MiddleName.Contains(term) ||
                            x.FinalizedBy.Division.Name.Contains(term) ||
                            x.FinalizedBy.Division.Department.Name.Contains(term) ||
                            x.FinalizedBy.Paygrade.Value.Contains(term) ||
                            x.FinalizedBy.UIC.Value.Contains(term) ||
                            x.FinalizedBy.Designation.Value.Contains(term));
                    }
                }
            }

            if (!String.IsNullOrWhiteSpace(command))
            {
                if (Guid.TryParse(command, out Guid id))
                {
                    query = query.Where(x => x.Command.Id == id);
                }
                else
                {
                    var terms = command.Split(',', ' ', ';', '-');

                    Expression<Func<MusterCycle, bool>> predicate = x => x.Command.Name.Contains(terms.First());

                    foreach (var term in terms.Skip(1))
                    {
                        predicate = predicate.Or(x => x.Command.Name.Contains(term));
                    }

                    query = query.Where(predicate);
                }
            }

            if (from.HasValue && !to.HasValue)
                query = query.Where(x => x.Range.Start >= from || x.Range.End >= from);
            else if (to.HasValue && !from.HasValue)
                query = query.Where(x => x.Range.Start <= to || x.Range.End <= to);
            else if (to.HasValue && to.HasValue)
                query = query.Where(x => x.Range.Start <= to && x.Range.End >= from);

            if (isFinalized.HasValue)
                query = query.Where(x => x.IsFinalized == isFinalized);

            var result = query
                .OrderByDescending(x => x.Range.Start)
                .Take(limit)
                .Select(cycle =>
                    new DTOs.MusterCycle.Get
                    {
                        Command = cycle.Command.Id,
                        FinalizedBy = cycle.FinalizedBy == null ? null : (Guid?)cycle.FinalizedBy.Id,
                        Id = cycle.Id,
                        IsFinalized = cycle.IsFinalized,
                        Range = cycle.Range,
                        TimeFinalized = cycle.TimeFinalized
                    }
                )
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Retrieves the current muster cycle associated with the client's command.
        /// </summary>
        /// <returns></returns>
        [HttpGet("current")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.MusterCycle.Get))]
        public IActionResult GetCurrent()
        {
            var musterCycle = User.Command.CurrentMusterCycle;

            return Ok(new DTOs.MusterCycle.Get
            {
                Command = musterCycle.Command.Id,
                FinalizedBy = musterCycle.FinalizedBy == null ? null : (Guid?)musterCycle.FinalizedBy.Id,
                Id = musterCycle.Id,
                IsFinalized = musterCycle.IsFinalized,
                Range = musterCycle.Range,
                TimeFinalized = musterCycle.TimeFinalized
            });
        }

        /// <summary>
        /// Retrieves the muster cycle identified by the given Id.
        /// </summary>
        /// <param name="id">The id of the muster cycle to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.MusterCycle.Get))]
        public IActionResult Get(Guid id)
        {
            var musterCycle = DBSession.Get<MusterCycle>(id);
            if (musterCycle == null)
                return NotFound();

            return Ok(new DTOs.MusterCycle.Get
            {
                Command = musterCycle.Command.Id,
                FinalizedBy = musterCycle.FinalizedBy == null ? null : (Guid?)musterCycle.FinalizedBy.Id,
                Id = musterCycle.Id,
                IsFinalized = musterCycle.IsFinalized,
                Range = musterCycle.Range,
                TimeFinalized = musterCycle.TimeFinalized
            });
        }

        /// <summary>
        /// Updates the muster cycle identified by the given Id.  The only property that is valid for update is isFinalized.  False = close muster.  True = reopen muster.
        /// </summary>
        /// <param name="id">The id of the muster cycle to retrieve.</param>
        /// <param name="dto">The dto containing the information required to patch a muster cycle.</param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.MusterCycle.Get))]
        public IActionResult Patch(Guid id, [FromBody]DTOs.MusterCycle.Patch dto)
        {
            if (dto == null)
                return BadRequest();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var musterCycle = DBSession.Get<MusterCycle>(id);
            if (musterCycle == null)
                return NotFound();

            if (CallTime > musterCycle.Range.End)
                return BadRequest("You may not patch a muster cycle whose end time has already passed.");

            using (var transaction = DBSession.BeginTransaction())
            {
                if (musterCycle.IsFinalized && !dto.IsFinalized)
                {
                    //The client wants to reopen the muster.
                    musterCycle.IsFinalized = false;

                    //Also clean out all the muster information entries since we don't need them anymore.
                    foreach (var entry in musterCycle.MusterEntries)
                    {
                        DBSession.Delete(entry.ArchiveInformation);
                        entry.ArchiveInformation = null;
                    }

                    Events.EventManager.OnMusterReopened(new Events.Args.MusterCycleEventArgs
                    {
                        MusterCycle = musterCycle
                    }, this);
                }
                else if (!musterCycle.IsFinalized && dto.IsFinalized)
                {
                    //The client wants to close the muster.
                    musterCycle.IsFinalized = true;
                    musterCycle.FinalizedBy = User;
                    musterCycle.TimeFinalized = CallTime;

                    //Set all the muster informations as well.
                    foreach (var entry in musterCycle.MusterEntries)
                    {
                        entry.ArchiveInformation = new MusterArchiveInformation
                        {
                            Command = entry.Person.Command?.Name,
                            Department = entry.Person.Department?.Name,
                            Designation = entry.Person.Designation?.Value,
                            Division = entry.Person.Division?.Name,
                            Id = Guid.NewGuid(),
                            MusterEntry = entry,
                            Paygrade = entry.Person.Paygrade?.Value,
                            UIC = entry.Person.UIC?.Value
                        };
                    }

                    Events.EventManager.OnMusterOpened(new Events.Args.MusterCycleEventArgs
                    {
                        MusterCycle = musterCycle
                    }, this);
                }

                DBSession.Update(musterCycle);
                transaction.Commit();
            }

            return Ok(new DTOs.MusterCycle.Get
            {
                Command = musterCycle.Command.Id,
                FinalizedBy = musterCycle.FinalizedBy == null ? null : (Guid?)musterCycle.FinalizedBy.Id,
                Id = musterCycle.Id,
                IsFinalized = musterCycle.IsFinalized,
                Range = musterCycle.Range,
                TimeFinalized = musterCycle.TimeFinalized
            });
        }
    }
}
