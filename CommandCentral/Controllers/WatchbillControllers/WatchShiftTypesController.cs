using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Enums;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.WatchbillControllers
{
    /// <summary>
    /// Provides access to the shift types collection.
    /// </summary>
    public class WatchShiftTypesController : CommandCentralController
    {
        /// <summary>
        /// Queries the watch shift types collection.
        /// </summary>
        /// <param name="command">A command query for the command for which the watch shift type was made.</param>
        /// <param name="name">A string query for the the name of the watch shift type.</param>
        /// <param name="description">A string query for the desc of a watch shift type.</param>
        /// <param name="qualification">An exact enum query for the qualification this watch shift type requires a person to have.</param>
        /// <param name="limit">[Optional][Default = 1000] Instructs the service to return no more than this number of results.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<DTOs.WatchShiftType.Get>), 200)]
        public IActionResult Get([FromQuery] string command, [FromQuery] string name, [FromQuery] string description,
            [FromQuery] string qualification, [FromQuery] int limit = 1000)
        {
            var predicate = ((Expression<Func<WatchShiftType, bool>>) null)
                .AddStringQueryExpression(x => x.Name, name)
                .AddStringQueryExpression(x => x.Description, description)
                .AddExactEnumQueryExpression(x => x.Qualification, qualification)
                .AddCommandQueryExpression(x => x.Command, command);

            var results = DBSession.Query<WatchShiftType>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .OrderBy(x => x.Command)
                .Take(limit)
                .ToList()
                .Where(x => User.CanReturn(x))
                .Select(x => new DTOs.WatchShiftType.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Retrieves a watch shift type.
        /// </summary>
        /// <param name="id">The id of the item to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DTOs.WatchShiftType.Get), 200)]
        public IActionResult Get(Guid id)
        {
            if (!TryGet(id, out WatchShiftType item))
                return NotFoundParameter(id, nameof(id));

            if (!User.CanReturn(item))
                return Forbid("You can't view this item.");

            return Ok(new DTOs.WatchShiftType.Get(item));
        }

        /// <summary>
        /// Creates a new watch shift type.
        /// </summary>
        /// <param name="dto">A dto containing the information needed to create a new watch shift type.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(DTOs.WatchShiftType.Get), 201)]
        public IActionResult Post([FromBody] DTOs.WatchShiftType.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!TryGet(dto.Command, out Command command))
                return NotFoundParameter(dto.Command, nameof(dto.Command));

            var shiftType = new WatchShiftType
            {
                Name = dto.Name,
                Description = dto.Description,
                Id = Guid.NewGuid(),
                Qualification = dto.Qualification,
                Command = command
            };

            var result = shiftType.Validate();
            if (!result.IsValid)
                return BadRequestWithValidationErrors(result);

            if (!User.CanEdit(shiftType))
                return Forbid("You can't create this shift type.");

            Save(shiftType);
            LogEntityCreation(shiftType);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = shiftType.Id}, new DTOs.WatchShiftType.Get(shiftType));
        }

        /// <summary>
        /// Modifies a watch shift type.
        /// </summary>
        /// <param name="id">The id of the watch shift type to modify.</param>
        /// <param name="dto">A dto containing the information needed to modify a watch shift type.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DTOs.WatchShiftType.Get), 201)]
        public IActionResult Put(Guid id, [FromBody] DTOs.WatchShiftType.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!TryGet(id, out WatchShiftType type))
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(type))
                return Forbid("You can't edit this shift type.");

            type.Description = dto.Description;
            type.Name = dto.Name;
            type.Qualification = dto.Qualification;

            var results = type.Validate();
            if (!results.IsValid)
                return BadRequestWithValidationErrors(results);

            LogEntityModification(type);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = type.Id}, new DTOs.WatchShiftType.Get(type));
        }

        /// <summary>
        /// Deletes the given watch shift type.
        /// </summary>
        /// <param name="id">The id of the item to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (!TryGet(id, out WatchShiftType type))
                return NotFoundParameter(id, nameof(id));
            
            if (!User.CanEdit(type))
                return Forbid("You can't delete this shift type.");

            Delete(type);
            LogEntityDeletion(type);
            CommitChanges();

            return NoContent();
        }
    }
}