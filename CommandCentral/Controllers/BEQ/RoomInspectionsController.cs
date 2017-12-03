using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.BEQ;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.BEQ
{
    /// <summary>
    /// Provides access to the room inspections collection.  Requires access to the BEQ chain of command in most cases.
    /// </summary>
    public class RoomInspectionsController : CommandCentralController
    {
        /// <summary>
        /// Queries the room inspections collection.  Requires the BEQ chain of command in most cases.
        /// </summary>
        /// <param name="time">A time range query for the time the room inspection was conducted.</param>
        /// <param name="room">An entity query for the room.</param>
        /// <param name="person">A person query for the person for whom the inspection was conducted.</param>
        /// <param name="inspectedBy">A person query for the collection of persons who conducted the inspection.</param>
        /// <param name="score">An integer query for the score given to the room inspection.</param>
        /// <param name="limit">[Optional][Default = 1000] Instructs the service to return no more than this number of results.</param>
        /// <returns></returns>
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
                .AddIntQueryExpression(x => x.Score, score)
                .AddEntityIdQueryExpression(x => x.Room, room);

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

        /// <summary>
        /// Retrieves a room inspection with the given id.
        /// </summary>
        /// <param name="id">The id of the room inspection to retrieve.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a new room inspection.
        /// </summary>
        /// <param name="dto">A dto containing the information needed to create a room inspection.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(DTOs.RoomInspection.Get))]
        public IActionResult Post([FromBody] DTOs.RoomInspection.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var inspectedByList = new List<Person>();
            foreach (var inspectedById in dto.InspectedBy)
            {
                var inspectedBy = DBSession.Get<Person>(inspectedById);
                if (inspectedBy == null)
                    return NotFoundParameter(dto.InspectedBy, nameof(dto.InspectedBy));

                inspectedByList.Add(inspectedBy);
            }

            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFoundParameter(dto.Person, nameof(dto.Person));

            var room = DBSession.Get<Room>(dto.Room);
            if (room == null)
                return NotFoundParameter(dto.Room, nameof(dto.Room));

            var roomInspection = new RoomInspection
            {
                Id = Guid.NewGuid(),
                InspectedBy = inspectedByList,
                Person = person,
                Room = room,
                Score = dto.Score,
                Time = dto.Time
            };

            if (!User.CanEdit(roomInspection))
                return Forbid("You can't add a room inspection.");

            var results = roomInspection.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(roomInspection);
            LogEntityCreation(roomInspection);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = roomInspection.Id},
                new DTOs.RoomInspection.Get(roomInspection));
        }

        /// <summary>
        /// Modifies a room inspection.
        /// </summary>
        /// <param name="id">Id of the room inspection to modify.</param>
        /// <param name="dto">A dto containing all of the information needed to modify a room inspection.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.RoomInspection.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.RoomInspection.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var roomInspection = DBSession.Get<RoomInspection>(id);
            if (roomInspection == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(roomInspection))
                return Forbid("You can't edit this room inspection.");

            roomInspection.Score = dto.Score;

            LogEntityModification(roomInspection);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = roomInspection.Id},
                new DTOs.RoomInspection.Get(roomInspection));
        }

        /// <summary>
        /// Deletes a room inspection.
        /// </summary>
        /// <param name="id">The id of the room inspection to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var roomInspection = DBSession.Get<RoomInspection>(id);
            if (roomInspection == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(roomInspection))
                return Forbid("You can't edit this room inspection.");

            DBSession.Delete(roomInspection);
            LogEntityDeletion(roomInspection);
            CommitChanges();

            return NoContent();
        }
    }
}