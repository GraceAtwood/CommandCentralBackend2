using System;
using System.Linq;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.WatchbillControllers
{
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
        
        // TODO: Get Query endpoint

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.WatchShift.Get))]
        public IActionResult Post([FromBody] DTOs.WatchShift.Post dto)
        {
            if (dto == null)
                return BadRequest();
            
            // TODO: Check to make sure they're allowed to add this shift!

            var shift = new WatchShift
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                ShiftType = DBSession.Get<WatchShiftType>(dto.ShiftType),
                Range = dto.Range,
                Watchbill = DBSession.Get<Watchbill>(dto.Watchbill)
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
                return BadRequest();

            var shift = DBSession.Get<WatchShift>(id);
            if (shift == null)
                return NotFoundParameter(id, nameof(id));
            
            // TODO: Check permissions on this shift!

            shift.Title = dto.Title;
            shift.Range = dto.Range;
            shift.ShiftType = DBSession.Get<WatchShiftType>(dto.ShiftType);

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

            // TODO: Check permissions
            
            DBSession.Delete(shift);
            
            CommitChanges();

            return NoContent();
        }
    }
}