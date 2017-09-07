using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.Entities.CollateralDutyTracking;
using CommandCentral.Enums;
using CommandCentral.Events;
using CommandCentral.Events.Args;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.CollateralDutyTrackingControllers
{
    public partial class CollateralDutiesController : CommandCentralController
    {
        [HttpGet("{dutyId}/Membership")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.CollateralDutyMembership.Get>))]
        public IActionResult GetMembership(Guid dutyId, [FromQuery] string level, [FromQuery] string role)
        {
            if (DBSession.Query<CollateralDuty>().Count(x => x.Id == dutyId) == 0)
                return NotFoundParameter(dutyId, nameof(dutyId));

            var predicate = ((Expression<Func<CollateralDutyMembership, bool>>) null)
                .AddExactEnumQueryExpression(x => x.Level, level)
                .AddExactEnumQueryExpression(x => x.Role, role)
                .NullSafeAnd(x => x.CollateralDuty.Id == dutyId);

            var results = DBSession.Query<CollateralDutyMembership>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToList()
                .Select(x => new DTOs.CollateralDutyMembership.Get(x))
                .ToList();

            return Ok(results);
        }

        [HttpPost("{dutyId}/Membership")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.CollateralDutyMembership.Get))]
        public IActionResult PostMembership(Guid dutyId, [FromBody] DTOs.CollateralDutyMembership.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (DBSession.Query<CollateralDuty>().Count(x => x.Id == dutyId) == 0)
                return NotFoundParameter(dutyId, nameof(dutyId));

            var clientMembership = DBSession.Query<CollateralDutyMembership>().SingleOrDefault(x =>
                x.CollateralDuty.Id == dutyId && x.Person == User &&
                (x.Role == CollateralRoles.Primary || x.Role == CollateralRoles.Secondary));

            if (!User.CanAccessSubmodules(SubModules.AdminTools) || clientMembership == null)
                return Forbid("In order to modify the membership of a collateral duty, you must either have access to " +
                              "the admin tools or be in the Primary or Secondary level of the collateral duty in question.");

            if (dto.Level > clientMembership.Level)
                return Forbid("In order to add a person to a collateral duty at a given level (Division, Department, or Command)," +
                              " your level in that collateral duty must be equal to or greater than that level.  Your level is " +
                              $"{clientMembership.Level} and the level you tried to add at was {dto.Level}.");
        }

        [HttpPut("id")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.CollateralDuty.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.CollateralDuty.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
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

        [HttpDelete("id")]
        [RequireAuthentication]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var item = DBSession.Get<CollateralDuty>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            DBSession.Delete(item);

            CommitChanges();

            EventManager.OnCollateralDutyDeleted(new CollateralDutyEventArgs
            {
                CollateralDuty = item
            }, this);

            return NoContent();
        }
    }
}