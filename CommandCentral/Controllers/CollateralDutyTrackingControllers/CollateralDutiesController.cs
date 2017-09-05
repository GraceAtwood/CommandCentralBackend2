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
    public class CollateralDutiesController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.CollateralDuty.Get>))]
        public IActionResult Get([FromQuery] string name, [FromQuery] string command)
        {
            var predicate = ((Expression<Func<CollateralDuty, bool>>) null)
                .AddCommandQueryExpression(x => x.Command, command)
                .AddStringQueryExpression(x => x.Name, name);

            var result = DBSession.Query<CollateralDuty>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .OrderByDescending(x => x.Name)
                .ToList()
                .Select(x => new DTOs.CollateralDuty.Get(x))
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