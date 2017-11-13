using System;
using System.Linq;
using CommandCentral.Entities;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.WatchbillControllers
{
    public class WatchAssignmentsController : CommandCentralController
    {
        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.WatchAssignments.Get))]
        public IActionResult Get(Guid id)
        {
            var assignment = DBSession.Get<WatchAssignment>(id);
            if (assignment == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.WatchAssignments.Get(assignment));
        }
        
        // TODO: Get Query Endpoint

        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.WatchAssignments.Get))]
        public IActionResult Post([FromBody] DTOs.WatchAssignments.Post dto)
        {
            if (dto == null)
                return BadRequest();
            
            // TODO: permissions check

            var assignment = new WatchAssignment
            {
                Id = Guid.NewGuid(),
                AssignedBy = User,
                DateAssigned = DateTime.Today,
                PersonAssigned = DBSession.Get<Person>(dto.PersonAssigned),
                WatchShift = DBSession.Get<WatchShift>(dto.Watchshift)
            };

            var result = assignment.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(assignment);
            
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = assignment.Id}, new DTOs.WatchAssignments.Get(assignment));
        }

        [HttpPatch("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.WatchAssignments.Get))]
        public IActionResult Patch(Guid id, [FromBody] DTOs.WatchAssignments.Patch dto)
        {
            if (dto == null)
                return BadRequest();

            var assignment = DBSession.Get<WatchAssignment>(id);
            if (assignment == null)
                return NotFound();

            if (!dto.IsAcknowledged)
                return Forbid("You can't unacknowledge a watch assignment.");
            
            //TODO: check permissions
            
            assignment.AcknowledgedBy = User;
            assignment.DateAcknowledged = DateTime.Today;
            assignment.IsAcknowledged = dto.IsAcknowledged;

            var result = assignment.Validate();

            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));
            
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = assignment.Id}, new DTOs.WatchAssignments.Get(assignment));
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var assignment = DBSession.Get<WatchAssignment>(id);
            if (assignment == null)
                return NotFound();

            // TODO: check permissions
            
            DBSession.Delete(assignment);
            
            CommitChanges();
            
            return NoContent();
        }
    }
}