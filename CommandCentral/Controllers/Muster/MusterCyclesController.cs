using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.Entities.Muster;
using CommandCentral.Enums;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.Muster
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
    [Produces("application/json")]
    [Consumes("application/json")]
    public class MusterCyclesController : CommandCentralController
    {
        /// <summary>
        /// Queries against the muster cycles.
        /// A muster cycle that has isFinalized=false but has FinalizedBy set to a person is a muster cycle that was reopened.  In this instance, FinalizedBy will contain the last person to finalize it, not the person that reopened it.
        /// </summary>
        /// <param name="range">Defines a time range query for the time range of a muster cycle.</param>
        /// <param name="isFinalized">true/false</param>
        /// <param name="wasFinalizedBySystem">true/false</param>
        /// <param name="finalizedBy">A person query for the person who finalized a muster cycle.</param>
        /// <param name="command">The command to which a muster cycle belongs.  Supports either Id selection or simple search-based query combined with a disjunction.</param>
        /// <param name="limit">[Default = 1000] Indicates that the api should return no more than this number of records.</param>
        /// <returns></returns>
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.MusterCycle.Get>))]
        public IActionResult Get([FromQuery] DTOs.DateTimeRangeQuery range, [FromQuery] bool? isFinalized, [FromQuery] bool? wasFinalizedBySystem,
            [FromQuery] string finalizedBy, [FromQuery] string command, [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var predicate = ((Expression<Func<MusterCycle, bool>>) null)
                .AddTimeRangeQueryExpression(x => x.Range, range)
                .AddNullableBoolQueryExpression(x => x.IsFinalized, isFinalized)
                .AddNullableBoolQueryExpression(x => x.WasFinalizedBySystem, wasFinalizedBySystem)
                .AddPersonQueryExpression(x => x.FinalizedBy, finalizedBy)
                .AddCommandQueryExpression(x => x.Command, command);

            var results = DBSession.Query<MusterCycle>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .OrderByDescending(x => x.Range.Start)
                .Take(limit)
                .ToList()
                .Select(x => new DTOs.MusterCycle.Get(x))
                .ToList();

            return Ok(results);
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
            return Ok(new DTOs.MusterCycle.Get(User.Command.CurrentMusterCycle));
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
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.MusterCycle.Get(musterCycle));
        }

        /// <summary>
        /// Updates the muster cycle identified by the given Id.  The only property that is valid for update is isFinalized.  False = close muster.  True = reopen muster.
        /// </summary>
        /// <param name="id">The id of the muster cycle to retrieve.</param>
        /// <param name="dto">The dto containing the information required to patch a muster cycle.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.MusterCycle.Get))]
        public IActionResult Put(Guid id, [FromBody]DTOs.MusterCycle.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var musterCycle = DBSession.Get<MusterCycle>(id);
            if (musterCycle == null)
                return NotFoundParameter(id, nameof(id));

            if (CallTime > musterCycle.Range.End)
                return BadRequest("You may not update a muster cycle whose end time has already passed.");

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
                    musterCycle.TimeFinalized = DateTime.UtcNow;
                    musterCycle.FinalizedBy = User;
                    musterCycle.WasFinalizedBySystem = true;

                    foreach (var entry in musterCycle.MusterEntries)
                    {
                        entry.ArchiveInformation = new MusterArchiveInformation(User, entry);
                    }

                    Events.EventManager.OnMusterFinalized(new Events.Args.MusterCycleEventArgs
                    {
                        MusterCycle = musterCycle
                    }, this);
                }

                DBSession.Update(musterCycle);
                transaction.Commit();
            }

            return Ok(new DTOs.MusterCycle.Get(musterCycle));
        }
    }
}
