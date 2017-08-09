using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities.Muster;
using CommandCentral.Authorization;
using CommandCentral.Enums;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class MusterCycleController : CommandCentralController
    {
        /*[HttpGet]
        public IActionResult Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] )
        {
        }*/

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var musterCycle = DBSession.Get<MusterCycle>(id);
            if (musterCycle == null)
                return NotFound();

            return Ok(new DTOs.MusterCycle.Get
            {
                Command = musterCycle.Command.Id,
                FinalizedBy = musterCycle.FinalizedBy.Id,
                Id = musterCycle.Id,
                IsFinalized = musterCycle.IsFinalized,
                Range = musterCycle.Range,
                TimeFinalized = musterCycle.TimeFinalized
            });
        }

        [HttpPatch("{id}")]
        public IActionResult Patch(Guid id, [FromBody]DTOs.MusterCycle.Patch dto)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var musterCycle = DBSession.Get<MusterCycle>(id);
            if (musterCycle == null)
                return NotFound();

            if (musterCycle.IsFinalized && !dto.IsFinalized)
            {
                //The client wants to reopen the muster.
                musterCycle.IsFinalized = false;

                Events.EventManager.OnMusterReopened(new Events.Args.MusterCycleEventArgs
                {
                    MusterCycle = musterCycle
                }, this);
            }
            else if (!musterCycle.IsFinalized && dto.IsFinalized)
            {
                //The client wants to close the muster.
                musterCycle.IsFinalized = true;

                Events.EventManager.OnMusterOpened(new Events.Args.MusterCycleEventArgs
                {
                    MusterCycle = musterCycle
                }, this);
            }

            return Ok(new DTOs.MusterCycle.Get
            {
                Command = musterCycle.Command.Id,
                FinalizedBy = musterCycle.FinalizedBy.Id,
                Id = musterCycle.Id,
                IsFinalized = musterCycle.IsFinalized,
                Range = musterCycle.Range,
                TimeFinalized = musterCycle.TimeFinalized
            });
        }
    }
}
