using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Entities.CollateralDutyTracking;
using CommandCentral.Enums;
using CommandCentral.Events;
using CommandCentral.Events.Args;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.CollateralDutyTrackingControllers
{
    /// <summary>
    /// Provides access to the collateral duties collection which tracks which collateral duties a person is in via the membership collection.
    /// Collateral duties are not intended to replace the permissions system and membership in a collateral duty does not confer the permissions that group grants.
    /// </summary>
    public class CollateralDutiesController : CommandCentralController
    {
        /// <summary>
        /// Queries the coll duties collection.
        /// </summary>
        /// <param name="name">A string query for the name of a collateral duty.</param>
        /// <param name="command">A command query for the command of a collateral duty.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.CollateralDuty.Get>))]
        public IActionResult Get([FromQuery] string name, [FromQuery] string command)
        {
            var predicate = ((Expression<Func<CollateralDuty, bool>>) null)
                .AddCommandQueryExpression(x => x.Command, command)
                .AddStringQueryExpression(x => x.Name, name);

            var result = DBSession.Query<CollateralDuty>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .OrderByDescending(x => x.Name)
                .ToList()
                .Select(x => new DTOs.CollateralDuty.Get(x))
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a single collateral duty.
        /// </summary>
        /// <param name="id">The id of the coll duty to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.CollateralDuty.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<CollateralDuty>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.CollateralDuty.Get(item));
        }

        /// <summary>
        /// Creates a new coll duty.  Client must have access to the admin tools.
        /// </summary>
        /// <param name="dto">A dto containing the information required to create a coll duty.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.CollateralDuty.Get))]
        public IActionResult Post([FromBody] DTOs.CollateralDuty.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.SpecialPermissions.Contains(SpecialPermissions.AdminTools))
                return Forbid();

            var item = new CollateralDuty
            {
                Command = User.Division.Department.Command,
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(item);

            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.CollateralDuty.Get(item));
        }

        /// <summary>
        /// Modifies a collateral duty.
        /// </summary>
        /// <param name="id">The id of the collateral duty to modify.</param>
        /// <param name="dto">A dto containing the information needed for modification.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.CollateralDuty.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.CollateralDuty.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.SpecialPermissions.Contains(SpecialPermissions.AdminTools))
                return Forbid();

            var item = DBSession.Get<CollateralDuty>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            item.Name = dto.Name;
            
            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));
            
            CommitChanges();
            
            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.CollateralDuty.Get(item));
        }

        /// <summary>
        /// Removes a collateral duty and all of the memberships associated with it.
        /// </summary>
        /// <param name="id">The id of the coll duty to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (!User.SpecialPermissions.Contains(SpecialPermissions.AdminTools))
                return Forbid();

            var item = DBSession.Get<CollateralDuty>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            Delete(item);

            EventManager.OnCollateralDutyDeleted(new CollateralDutyEventArgs
            {
                CollateralDuty = item
            }, this);
            
            CommitChanges();

            return NoContent();
        }
    }
}