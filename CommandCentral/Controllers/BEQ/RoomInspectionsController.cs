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
    public class RoomInspectionsController : CommandCentralController
    {
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.RoomInspection.Get>))]
        public IActionResult Get([FromQuery] DTOs.DateTimeRangeQuery time, [FromQuery] string room,
            [FromQuery] string person, [FromQuery] string inspectedBy, [FromQuery] string score,
            [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var predicate = ((Expression<Func<RoomInspection, bool>>) null)
                .AddDateTimeQueryExpression(x => x.Time, time)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddIntQueryExpression(x => x.Score, score);

            if (!String.IsNullOrWhiteSpace(room))
            {
                predicate.NullSafeAnd(room.SplitByOr().Select(phrase =>
                    {
                        if (Guid.TryParse(phrase, out var id))
                            return ((Expression<Func<RoomInspection, bool>>) null).NullSafeAnd(x => x.Room.Id == id);

                        return null;
                    })
                    .Where(x => x != null)
                    .Aggregate<Expression<Func<RoomInspection, bool>>, Expression<Func<RoomInspection, bool>>>(null,
                        (current, subPredicate) => current.NullSafeOr(subPredicate)));
            }

            if (!String.IsNullOrWhiteSpace(inspectedBy))
            {
                predicate.NullSafeAnd(inspectedBy.SplitByOr().Select(phrase =>
                    {
                        if (Guid.TryParse(phrase, out var id))
                            return ((Expression<Func<RoomInspection, bool>>) null).NullSafeAnd(x =>
                                x.InspectedBy.Any(y => y.Id == id));

                        return null;
                    })
                    .Where(x => x != null)
                    .Aggregate<Expression<Func<RoomInspection, bool>>, Expression<Func<RoomInspection, bool>>>(null,
                        (current, subPredicate) => current.NullSafeOr(subPredicate)));
            }

            var results = DBSession.Query<RoomInspection>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .OrderByDescending(x => x.Time)
                .Take(limit)
                .ToList()
                .Where(User.CanReturn)
                .Select(x => new DTOs.RoomInspection.Get(x))
                .ToList();

            return Ok(results);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.RoomInspection.Get))]
        public IActionResult Get(Guid id)
        {
            var roomInspection = DBSession.Get<RoomInspection>(id);
            if (roomInspection == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanReturn(roomInspection))
                return Forbid("You can't view this room inspection.");

            return Ok(new DTOs.RoomInspection.Get(roomInspection));
        }

        [HttpPost]
        [ProducesResponseType(200, Type = typeof(DTOs.RoomInspection.Get))]
        public IActionResult Post([FromBody] DTOs.RoomInspection.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();
            
            throw new NotImplementedException();
        }
    }
}