using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.Entities.CollateralDutyTracking;
using CommandCentral.Enums;
using CommandCentral.Events;
using CommandCentral.Events.Args;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.CollateralDutyTrackingControllers
{
    public partial class CollateralDutiesController : CommandCentralController
    {
        [HttpGet("{dutyId}/Membership")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.CollateralDuty.Get>))]
        public IActionResult Get(Guid dutyId, [FromQuery] string level, [FromQuery] string role)
        {
            if (DBSession.Query<CollateralDuty>().Count(x => x.Id == dutyId) == 0)
                return NotFoundParameter(dutyId, nameof(dutyId));

            var results = DBSession.Query<CollateralDutyMembership>()
                .Where(x => x.CollateralDuty.Id == dutyId)
                .ToList();

            return Ok(result);
        }

        [HttpGet("id")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.CollateralDuty.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<CollateralDuty>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.CollateralDuty.Get(item));
        }

        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.CollateralDuty.Get))]
        public IActionResult Post([FromBody] DTOs.CollateralDuty.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = new CollateralDuty
            {
                Command = User.Command,
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(item);

            CommitChanges();

            EventManager.OnCollateralDutyCreated(new CollateralDutyEventArgs
            {
                CollateralDuty = item
            }, this);

            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.CollateralDuty.Get(item));
        }

        [HttpPut("id")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.CollateralDuty.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.CollateralDuty.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = DBSession.Get<CollateralDuty>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            item.Name = dto.Name;
            
            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));
            
            CommitChanges();
            
            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.CollateralDuty.Get(item));
        }

        [HttpDelete("id")]
        [RequireAuthentication]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = DBSession.Get<CollateralDuty>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            DBSession.Delete(item);
            
            CommitChanges();
            
            EventManager.OnCollateralDutyDeleted(new CollateralDutyEventArgs
            {
                CollateralDuty = item
            }, this);

            return NoContent();
        }
    }
}