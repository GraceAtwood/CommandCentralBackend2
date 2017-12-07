using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.DTOs;
using CommandCentral.Entities;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Enums;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.WatchbillControllers
{
    public class WatchAssignmentsController : CommandCentralController
    {
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.WatchAssignments.Get>))]
        public IActionResult Get([FromQuery] string watchShift, [FromQuery] string personAssigned,
            [FromQuery] string assignedBy, [FromQuery] string acknowledgedBy,
            [FromQuery] DateTimeRangeQuery dateAssigned, [FromQuery] DateTimeRangeQuery dateAcknowledged,
            [FromQuery] bool? isAcknowledged)
        {
            var predicate = ((Expression<Func<WatchAssignment, bool>>) null)
                .AddPersonQueryExpression(x => x.PersonAssigned, personAssigned)
                .AddPersonQueryExpression(x => x.AssignedBy, assignedBy)
                .AddPersonQueryExpression(x => x.AcknowledgedBy, acknowledgedBy)
                .AddDateTimeQueryExpression(x => x.DateAcknowledged, dateAcknowledged)
                .AddDateTimeQueryExpression(x => x.DateAssigned, dateAssigned)
                .AddNullableBoolQueryExpression(x => x.IsAcknowledged, isAcknowledged);

            if (!String.IsNullOrWhiteSpace(watchShift))
            {
                predicate = predicate.NullSafeAnd(watchShift.SplitByOr()
                    .Select(phrase =>
                    {
                        if (Guid.TryParse(phrase, out var id))
                            return ((Expression<Func<WatchAssignment, bool>>) null).And(x => x.WatchShift.Id == id);

                        return x => x.WatchShift.Title.Contains(phrase);
                    })
                    .Aggregate<Expression<Func<WatchAssignment, bool>>, Expression<Func<WatchAssignment, bool>>>(null,
                        (current, subPredicate) => current.NullSafeOr(subPredicate)));
            }

            var results = DBSession.Query<WatchAssignment>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToList()
                .Select(x => new DTOs.WatchAssignments.Get(x))
                .ToList();

            return Ok(results);
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

            if (watchShift.Watchbill.Command != User.Division.Department.Command)
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

            switch (User.GetHighestAccessLevels()[ChainsOfCommand.QuarterdeckWatchbill])
            {
                case ChainOfCommandLevels.Command:
                {
                    if (User.Division.Department.Command != assignment.WatchShift.DivisionAssignedTo.Department.Command)
                        return Forbid("You may not delete a watch assignment.");
                    break;
                }
                case ChainOfCommandLevels.Department:
                {
                    if (User.Division.Department != assignment.WatchShift.DivisionAssignedTo.Department)
                        return Forbid("You may not delete a watch assignment.");

                    if (assignment.WatchShift.Watchbill.Phase != WatchbillPhases.Assignment)
                        return Forbid("You may not delete a watch assignment.");

                    break;
                }
                case ChainOfCommandLevels.Division:
                {
                    if (User.Division != assignment.WatchShift.DivisionAssignedTo)
                        return Forbid("You may not delete a watch assignment.");

                    if (assignment.WatchShift.Watchbill.Phase != WatchbillPhases.Assignment)
                        return Forbid("You may not delete a watch assignment.");

                    break;
                }
                case ChainOfCommandLevels.None:
                {
                    return Forbid("You may not delete a watch assignment.");
                }
                default:
                    throw new NotImplementedException("Fell to switch in DELETE of watch assignments.");
            }

            Delete(assignment);
            CommitChanges();

            return NoContent();
        }
    }
}