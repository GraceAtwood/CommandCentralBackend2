using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Enums;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.WatchbillControllers
{
    public class WatchShiftTypesController : CommandCentralController
    {
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.WatchShiftType.Get>))]
        public IActionResult Get()
        {
            var items = DBSession.Query<WatchShiftType>().ToList();

            return Ok(items.Select(x => new DTOs.WatchShiftType.Get(x)));
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.WatchShiftType.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<WatchShiftType>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.WatchShiftType.Get(item));
        }

        [HttpPost]
        [ProducesResponseType(200, Type = typeof(DTOs.WatchShiftType.Get))]
        public IActionResult Post([FromBody] DTOs.WatchShiftType.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();
            
            if (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill] != ChainOfCommandLevels.Command)
                return Forbid("You may not add a shift type unless you are a command watchbill coordinator.");
            
            var shiftType = new WatchShiftType
            {
                Name = dto.Name,
                Description = dto.Description,
                Id = Guid.NewGuid(),
                Qualification = dto.Qualification
            };

            var result = shiftType.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(shiftType);
            CommitChanges();
            
            return CreatedAtAction(nameof(Get), new {id = shiftType.Id}, new DTOs.WatchShiftType.Get(shiftType));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill] != ChainOfCommandLevels.Command)
                return Forbid();
            
            var shiftType = DBSession.Get<WatchShiftType>(id);
            if (shiftType == null)
                return NotFoundParameter(id, nameof(id));

            Delete(shiftType);
            CommitChanges();

            return NoContent();
        }
    }
}