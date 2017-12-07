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
    /// <summary>
    /// Provides access to the buildings resource.  Requires access, in most cases, to the BEQ chain of command.
    /// </summary>
    public class BuildingsController : CommandCentralController
    {
        /// <summary>
        /// Queries the buildings collection.
        /// </summary>
        /// <param name="description">A string query for the description of a building.</param>
        /// <param name="name">A string query for the name of a building.</param>
        /// <param name="command">A command query for the command of a building.</param>
        /// <returns></returns>
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
                .Select(x => new DTOs.Building.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Retrieves the building identified by the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a new building.
        /// </summary>
        /// <param name="dto">Contains the information needed to create a new building.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.Building.Get))]
        public IActionResult Post([FromBody] DTOs.Building.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var command = DBSession.Get<Command>(dto.Command);
            if (command == null)
                return NotFoundParameter(dto.Command, nameof(command));

            var building = new Building
            {
                Command = command,
                Description = dto.Description,
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            if (!User.CanEdit(building))
                return Forbid("You can't add buildings.");
            
            var results = building.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(building);
            LogEntityCreation(building);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = building.Id}, new DTOs.Building.Get(building));
        }

        /// <summary>
        /// Updates a building.
        /// </summary>
        /// <param name="id">The id of the building to update.</param>
        /// <param name="dto">A dto containing all of the information needed to modify a building.</param>
        /// <returns></returns>
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
            
            var results = building.Validate();
            if (!results.IsValid)
                return BadRequest(results.Errors.Select(x => x.ErrorMessage));

            LogEntityModification(building);
            CommitChanges();
            
            return CreatedAtAction(nameof(Get), new {id = building.Id}, new DTOs.Building.Get(building));
        }

        /// <summary>
        /// Deletes the given building.
        /// </summary>
        /// <param name="id">The id of the building to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var building = DBSession.Get<Building>(id);
            if (building == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(building))
                return Forbid("You can't edit this building.");

            Delete(building);
            LogEntityDeletion(building);
            CommitChanges();

            return NoContent();
        }
    }
}