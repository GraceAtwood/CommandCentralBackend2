using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.DTOs.Custom;
using CommandCentral.Entities.BEQ;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.BEQ
{
    public class WorkOrderControllers : CommandCentralController
    {
        [HttpGet]
        [ProducesResponseType(typeof(List<DTOs.WorkOrder.Get>), 200)]
        public IActionResult Get([FromQuery] string body, [FromQuery] string location, [FromQuery] string room,
            [FromQuery] DateTimeRangeQuery timeSubmitted, [FromQuery] string submittedBy,
            [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));
            
            var predicate = ((Expression<Func<WorkOrder, bool>>) null)
                .AddStringQueryExpression(x => x.Body, body)
                .AddStringQueryExpression(x => x.Location, location)
                .AddPersonQueryExpression(x => x.SubmittedBy, submittedBy)
                .AddDateTimeQueryExpression(x => x.TimeSubmitted, timeSubmitted);

            if (!String.IsNullOrWhiteSpace(room))
            {
                predicate.NullSafeAnd(room.SplitByOr().Select(phrase =>
                    {
                        if (Guid.TryParse(phrase, out var id))
                            return ((Expression<Func<WorkOrder, bool>>) null).NullSafeAnd(x => x.RoomLocation.Id == id);

                        return null;
                    })
                    .Where(x => x != null)
                    .Aggregate<Expression<Func<WorkOrder, bool>>, Expression<Func<WorkOrder, bool>>>(null,
                        (current, subPredicate) => current.NullSafeOr(subPredicate)));
            }

            var results = DBSession.Query<WorkOrder>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .OrderByDescending(x => x.TimeSubmitted)
                .Take(limit)
                .ToList()
                .Where(x => User.CanReturn(x))
                .Select(x => new DTOs.WorkOrder.Get(x))
                .ToList();

            return Ok(results);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DTOs.WorkOrder.Get), 200)]
        public IActionResult Get(Guid id)
        {
            var workOrder = DBSession.Get<WorkOrder>(id);
            if (workOrder == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanReturn(workOrder))
                return Forbid("You can't view this work order.");

            return Ok(new DTOs.WorkOrder.Get(workOrder));
        }
        
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(DTOs.WorkOrder.Get))]
        public IActionResult Post([FromBody] DTOs.WorkOrder.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();
            
            Room roomLocation = null;
            if (dto.RoomLocation.HasValue)
            {
                roomLocation = DBSession.Get<Room>(dto.RoomLocation.Value);
                if (roomLocation == null)
                    return NotFoundParameter(dto.RoomLocation.Value, nameof(dto.RoomLocation));
            }
            
            var workOrder = new WorkOrder
            {
                Body = dto.Body,
                Id = Guid.NewGuid(),
                Location = dto.Location,
                RoomLocation = roomLocation,
                SubmittedBy = User,
                TimeSubmitted = CallTime
            };

            if (!User.CanEdit(workOrder))
                return Forbid("You can't add a work order.");

            var results = workOrder.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(workOrder);
            LogEntityCreation(workOrder);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = workOrder.Id},
                new DTOs.WorkOrder.Get(workOrder));
        }
        
        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.WorkOrder.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.WorkOrder.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var workOrder = DBSession.Get<WorkOrder>(id);
            if (workOrder == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(workOrder))
                return Forbid("You can't edit this work order.");

            workOrder.Body = dto.Body;
            workOrder.Location = dto.Location;
            
            Room roomLocation = null;
            if (dto.RoomLocation.HasValue)
            {
                roomLocation = DBSession.Get<Room>(dto.RoomLocation.Value);
                if (roomLocation == null)
                    return NotFoundParameter(dto.RoomLocation.Value, nameof(dto.RoomLocation));

                workOrder.RoomLocation = roomLocation;
            }
            
            LogEntityModification(workOrder);
            CommitChanges();
            
            return CreatedAtAction(nameof(Get), new {id = workOrder.Id},
                new DTOs.WorkOrder.Get(workOrder));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var workOrder = DBSession.Get<WorkOrder>(id);
            if (workOrder == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(workOrder))
                return Forbid("You can't edit this work order.");

            Delete(workOrder);
            LogEntityDeletion(workOrder);
            CommitChanges();

            return NoContent();
        }
    }
}