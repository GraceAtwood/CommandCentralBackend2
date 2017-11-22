using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.DTOs.CollateralDutyMembership;
using CommandCentral.Entities;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Enums;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.WatchbillControllers
{
    public class WatchAssignmentsController : CommandCentralController
    {
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.WatchAssignments.Get>))]
        public IActionResult Get()
        {
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.WatchAssignments.Get))]
        public IActionResult Get(Guid id)
        {
            var assignment = DBSession.Get<WatchAssignment>(id);
            if (assignment == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.WatchAssignments.Get(assignment));
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.WatchAssignments.Get))]
        public IActionResult Post([FromBody] DTOs.WatchAssignments.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var watchShift = DBSession.Get<WatchShift>(dto.Watchshift);
            if (watchShift == null)
                return NotFoundParameter(dto.Watchshift, nameof(dto.Watchshift));

            if (watchShift.Watchbill.Command != User.Command)
                return Forbid("You may not modify a watchbill outside of your command.");

            var personAssigned = DBSession.Get<Person>(dto.PersonAssigned);
            if (personAssigned == null)
                return NotFoundParameter(dto.PersonAssigned, nameof(dto.PersonAssigned));

            if (personAssigned.DutyStatus != DutyStatuses.Active)
                return BadRequest("You may not assign a person to a shift if they are not active duty.");

            if (!User.IsInChainOfCommand(personAssigned, ChainsOfCommand.QuarterdeckWatchbill))
                return Forbid("You may not assign this person to a watch - you are not in his or her " +
                              "watchbill chain of command!");

            //If the user isn't command level, then they can only make assignments in the assignment phase.
            //Command level gets to ignore this requirement.
            //Also, if the user isn't command level, they must adhere to the division assignment requirement.
            var highestLevel = User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill];
            if (highestLevel == ChainOfCommandLevels.Department || highestLevel == ChainOfCommandLevels.Division)
            {
                if (watchShift.Watchbill.Phase != WatchbillPhases.Assignment)
                    return Conflict("You may not assign a person to a shift unless that shift's " +
                                    "watchbill is in the 'Assignment' phase.");

                if (watchShift.DivisionAssignedTo != personAssigned.Division)
                    return Conflict("You may not assign this person to this watch shift because it's " +
                                    "assigned to a different division.");
            }

            var assignment = new WatchAssignment
            {
                Id = Guid.NewGuid(),
                AssignedBy = User,
                DateAssigned = CallTime,
                PersonAssigned = personAssigned,
                WatchShift = watchShift
            };

            var result = assignment.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(assignment);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = assignment.Id}, new DTOs.WatchAssignments.Get(assignment));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.WatchAssignments.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.WatchAssignments.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var assignment = DBSession.Get<WatchAssignment>(id);
            if (assignment == null)
                return NotFoundParameter(id, nameof(id));

            if (assignment.WatchShift.Watchbill == null)
                throw new Exception($"Failed to find the watchbill for the assignment with id '{assignment.Id}'!");

            if (assignment.WatchShift.Watchbill.Phase != WatchbillPhases.Publish && dto.IsAcknowledged)
                return BadRequest("You may not acknowledge watches until the owning watchbill's phase is 'Publish'.");

            //Does the client want to reassign the person for watch?
            if (dto.PersonAssigned.HasValue &&
                (assignment.PersonAssigned == null || assignment.PersonAssigned != null &&
                 assignment.PersonAssigned.Id != dto.PersonAssigned))
            {
                var personAssigned = DBSession.Get<Person>(dto.PersonAssigned);
                if (personAssigned == null)
                    return NotFoundParameter(dto.PersonAssigned, nameof(dto.PersonAssigned));

                if (personAssigned.DutyStatus != DutyStatuses.Active)
                    return BadRequest("You may not assign a person to a shift if they are not active duty.");

                if (!User.IsInChainOfCommand(personAssigned, ChainsOfCommand.QuarterdeckWatchbill))
                    return Forbid("You may not assign this person to a watch - you are not in his or her " +
                                  "watchbill chain of command!");

                //If the user isn't command level, then they can only make assignments in the assignment phase.
                //Command level gets to ignore this requirement.
                //Also, if the user isn't command level, they must adhere to the division assignment requirement.
                var highestLevel = User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill];
                if (highestLevel == ChainOfCommandLevels.Department || highestLevel == ChainOfCommandLevels.Division)
                {
                    if (assignment.WatchShift.Watchbill.Phase != WatchbillPhases.Assignment)
                        return Conflict("You may not assign a person to a shift unless that shift's " +
                                        "watchbill is in the 'Assignment' phase.");

                    if (assignment.WatchShift.DivisionAssignedTo != personAssigned.Division)
                        return Conflict("You may not assign this person to this watch shift because it's " +
                                        "assigned to a different division.");
                }

                //At this point, I think we're finally ready to assign the watch.
                assignment.PersonAssigned = personAssigned;
                assignment.AssignedBy = User;
                assignment.DateAssigned = CallTime;
                assignment.NumberOfAlertsSent = 0;
            }
            //Does the client want to unassign the watch? (set it to no one?)
            else if (!dto.PersonAssigned.HasValue && assignment.PersonAssigned != null)
            {
                //TOO BAD!  You can't fucking do that!  Delete the assignment instead.
                return RedirectToAction(nameof(Delete), new {id = assignment.Id});
            }

            if (dto.IsAcknowledged)
            {
                assignment.AcknowledgedBy = User;
                assignment.DateAcknowledged = CallTime;
                assignment.IsAcknowledged = true;
            }

            var result = assignment.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = assignment.Id}, new DTOs.WatchAssignments.Get(assignment));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var assignment = DBSession.Get<WatchAssignment>(id);
            if (assignment == null)
                return NotFoundParameter(id, nameof(id));

            //This let's us skip one roundtrip to the database instead of using lazy loading.
            var watchbill = DBSession.Query<Watchbill>()
                .SingleOrDefault(x => x.WatchShifts.Any(y => y.WatchAssignment.Id == assignment.Id));

            if (watchbill == null)
                throw new Exception($"Failed to find the watchbill for the assignment with id '{assignment.Id}'!");

            if (watchbill.Phase != WatchbillPhases.Assignment)
            {
                if (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill] !=
                    ChainOfCommandLevels.Command || User.Command != watchbill.Command)
                    return Forbid("This assignment's watchbill is not in the assignment phase; therefore, you must " +
                                  "be in the command level of the watchbill chain of command to delete a watch assignemnt.");
            }

            DBSession.Delete(assignment);
            CommitChanges();

            return NoContent();
        }
    }
}