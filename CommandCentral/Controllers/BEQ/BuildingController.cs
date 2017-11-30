using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.BEQ;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.BEQ
{
    public class BuildingController : CommandCentralController
    {
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Building.Get>))]
        public IActionResult Get([FromQuery] string description, [FromQuery] string name, [FromQuery] string command)
        {
            var predicate = ((Expression<Func<Building, bool>>) null)
                .AddStringQueryExpression(x => x.Description, description)
                .AddStringQueryExpression(x => x.Name, name)
                .AddCommandQueryExpression(x => x.Command, command);

            var results = DBSession.Query<Building>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToList()
                .Where(x => User.CanReturn(x))
                .ToList();

            return Ok(results);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.Building.Get))]
        public IActionResult Get(Guid id)
        {
            var building = DBSession.Get<Building>(id);
            if (building == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanReturn(building))
                return Forbid("You can't view this building.");

            return Ok(new DTOs.Building.Get(building));
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.Building.Get))]
        public IActionResult Post([FromBody] DTOs.Building.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var command = DBSession.Get<Command>(dto.Command);
            if (command == null)
                return NotFoundParameter(command, nameof(command));

            var building = new Building
            {
                Command = command,
                Description = dto.Description,
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            if (!User.CanEdit(building))
                return Forbid("You can't add buildings.");

            DBSession.Save(building);
            LogEntityCreation(building);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = building.Id}, new DTOs.Building.Get(building));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.Building.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.Building.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var building = DBSession.Get<Building>(id);
            if (building == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(building))
                return Forbid("You can't edit this building.");

            building.Description = dto.Description;
            building.Name = dto.Name;

            LogEntityModification(building);
            CommitChanges();
            
            return CreatedAtAction(nameof(Get), new {id = building.Id}, new DTOs.Building.Get(building));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var building = DBSession.Get<Building>(id);
            if (building == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(building))
                return Forbid("You can't edit this building.");

            DBSession.Delete(building);
            LogEntityDeletion(building);
            CommitChanges();

            return NoContent();
        }
    }
}