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
    public class RoomsController : CommandCentralController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="number"></param>
        /// <param name="personAssigned"></param>
        /// <param name="building"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Room.Get>))]
        public IActionResult Get([FromQuery] string level, [FromQuery] string number, [FromQuery] string personAssigned,
            [FromQuery] string building)
        {
            var predicate = ((Expression<Func<Room, bool>>) null)
                .AddIntQueryExpression(x => x.Level, level)
                .AddIntQueryExpression(x => x.Number, number)
                .AddPersonQueryExpression(x => x.PersonAssigned, personAssigned);

            if (!String.IsNullOrWhiteSpace(building))
            {
                predicate.NullSafeAnd(building.SplitByOr().Select(phrase =>
                {
                    if (Guid.TryParse(phrase, out var id))
                        return ((Expression<Func<Room, bool>>) null).NullSafeAnd(x => x.Building.Id == id);

                    return ((Expression<Func<Room, bool>>) null).NullSafeAnd(x =>
                        x.Building.Name.Contains(phrase) || x.Building.Description.Contains(phrase));
                }).Aggregate<Expression<Func<Room, bool>>, Expression<Func<Room, bool>>>(null,
                    (current, subPredicate) => current.NullSafeOr(subPredicate)));
            }

            var results = DBSession.Query<Room>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToList()
                .Where(User.CanReturn)
                .Select(x => new DTOs.Room.Get(x))
                .ToList();

            return Ok(results);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.Room.Get))]
        public IActionResult Get(Guid id)
        {
            var room = DBSession.Get<Room>(id);
            if (room == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanReturn(room))
                return Forbid("You can't view this room.");

            return Ok(new DTOs.Room.Get(room));
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.Room.Get))]
        public IActionResult Post([FromBody] DTOs.Room.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var building = DBSession.Get<Building>(dto.Building);
            if (building == null)
                return NotFoundParameter(dto.Building, nameof(dto.Building));

            Person personAssigned = null;
            if (dto.PersonAssigned.HasValue)
            {
                personAssigned = DBSession.Get<Person>(dto.PersonAssigned.Value);
                if (personAssigned == null)
                    return NotFoundParameter(dto.PersonAssigned.Value, nameof(dto.PersonAssigned));
            }

            var room = new Room
            {
                Building = building,
                Id = Guid.NewGuid(),
                Level = dto.Level,
                Number = dto.Number,
                PersonAssigned = personAssigned
            };

            if (!User.CanEdit(building, x => x.Rooms))
                return Forbid("You can't edit the rooms for that building.");

            var results = room.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(room);
            LogEntityCreation(room);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = room.Id}, new DTOs.Room.Get(room));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.Room.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.Room.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var room = DBSession.Get<Room>(id);
            if (room == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(room))
                return Forbid("You can't edit this room.");

            if (dto.PersonAssigned.HasValue)
            {
                var personAssigned = DBSession.Get<Person>(dto.PersonAssigned.Value);
                if (personAssigned == null)
                    return NotFoundParameter(dto.PersonAssigned.Value, nameof(dto.PersonAssigned));

                room.PersonAssigned = personAssigned;
            }
            
            var results = room.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            LogEntityModification(room);
            CommitChanges();
            
            return CreatedAtAction(nameof(Get), new {id = room.Id}, new DTOs.Room.Get(room));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var room = DBSession.Get<Room>(id);
            if (room == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(room))
                return Forbid("You can't edit this room.");

            DBSession.Delete(room);
            LogEntityDeletion(room);
            CommitChanges();

            return NoContent();
        }
    }
}