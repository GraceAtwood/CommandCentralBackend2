using System;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Enums;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.WatchbillControllers
{
    /// <summary>
    /// Provides access to the watch shifts of a watchbill.
    /// </summary>
    public class WatchShiftsController : CommandCentralController
    {
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.WatchShift.Get))]
        public IActionResult Get(Guid id)
        {
            var shift = DBSession.Get<WatchShift>(id);
            if (shift == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.WatchShift.Get(shift));
        }
        
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.WatchShift.Get))]
        public IActionResult Post([FromBody] DTOs.WatchShift.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill] != ChainOfCommandLevels.Command)
                return Forbid("You may not add shifts to a watchbill " +
                              "unless you are command level in the watchbill chain of command.");

            var watchbill = DBSession.Get<Watchbill>(dto.Watchbill);
            if (watchbill == null)
                return NotFoundParameter(dto.Watchbill, nameof(dto.Watchbill));

            if (watchbill.Phase != WatchbillPhases.Initial)
                return Conflict("You may not add a shift to a watchbill whose phase is not initial.");

            var shiftType = DBSession.Get<WatchShiftType>(dto.ShiftType);
            if (shiftType == null)
                return NotFoundParameter(dto.ShiftType, nameof(dto.ShiftType));

            var shift = new WatchShift
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                ShiftType = shiftType,
                Range = dto.Range,
                Watchbill = watchbill
            };

            var result = shift.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(shift);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = shift.Id}, new DTOs.WatchShift.Get(shift));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.WatchShift.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.WatchShift.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();
            
            if (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill] != ChainOfCommandLevels.Command)
                return Forbid("You may not add shifts to a watchbill " +
                              "unless you are command level in the watchbill chain of command.");

            var shift = DBSession.Get<WatchShift>(id);
            if (shift == null)
                return NotFoundParameter(id, nameof(id));
            
            if (shift.Watchbill.Phase != WatchbillPhases.Initial)
                return Conflict("You may not modify a shift of a watchbill whose phase is not initial.");

            var shiftType = DBSession.Get<WatchShiftType>(dto.ShiftType);
            if (shiftType == null)
                return NotFoundParameter(dto.ShiftType, nameof(dto.ShiftType));
            
            shift.Title = dto.Title;
            shift.Range = dto.Range;
            shift.ShiftType = shiftType;

            var result = shift.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));
            
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = shift.Id}, new DTOs.WatchShift.Get(shift));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var shift = DBSession.Get<WatchShift>(id);
            if (shift == null)
                return NotFoundParameter(id, nameof(id));

            DBSession.Delete(shift);
            CommitChanges();

            return NoContent();
        }
    }
}