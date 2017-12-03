using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
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
            [FromQuery] DTOs.DateTimeRangeQuery timeSubmitted, [FromQuery] string submittedBy,
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
    }
}