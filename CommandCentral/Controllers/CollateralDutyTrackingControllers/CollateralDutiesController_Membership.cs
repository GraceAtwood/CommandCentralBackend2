using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.Entities;
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
    public partial class CollateralDutiesController
    {
        /// <summary>
        /// Retrieves the membership collection associated with the identified coll duty.
        /// </summary>
        /// <param name="dutyId">The id of the collateral duty whose membership collection you want to retrieve.</param>
        /// <param name="level">An exact enum query for the level of a membership.</param>
        /// <param name="role">An exact enum query for the role of a membership.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a new membership in the membership collection of the identified coll duty.
        /// The client must have access to the admin tools or have a membership in the identified coll duty in the 
        /// Primary or Secondary role at a level equal to or greater than the level at which the client wants to create the membership.
        /// </summary>
        /// <param name="dutyId">The id of the collateral to which to add a membership.</param>
        /// <param name="dto">A dto containing the information needed to create a new membership.</param>
        /// <returns></returns>
        [HttpPost("{dutyId}/Membership")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.CollateralDutyMembership.Get))]
        public IActionResult PostMembership(Guid dutyId, [FromBody] DTOs.CollateralDutyMembership.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var duty = DBSession.Get<CollateralDuty>(dutyId);
            if (duty == null)
                return NotFoundParameter(dutyId, nameof(dutyId));

            var clientMembership = DBSession.Query<CollateralDutyMembership>().SingleOrDefault(x =>
                x.CollateralDuty.Id == dutyId && x.Person == User &&
                (x.Role == CollateralRoles.Primary || x.Role == CollateralRoles.Secondary));

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
            {
                if (clientMembership == null)
                {
                    return Forbid(
                        "In order to modify the membership of a collateral duty, you must either have access to " +
                        "the admin tools or be in the Primary or Secondary level of the collateral duty in question.");
                }
                if (dto.Level > clientMembership.Level)
                    return Forbid(
                        "In order to add a person to a collateral duty at a given level (Division, Department, or Command)," +
                        " your level in that collateral duty must be equal to or greater than that level.  Your level is " +
                        $"{clientMembership.Level} and the level you tried to add at was {dto.Level}.");
            }

            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFoundParameter(dto.Person, nameof(dto.Person));

            if (duty.Membership.Any(x => x.Person == person))
                return Conflict("Your given person is already in this collateral duty!  " +
                                "Please consider deleting the existing membership or modifying it.");

            var membership = new CollateralDutyMembership
            {
                CollateralDuty = duty,
                Id = Guid.NewGuid(),
                Level = dto.Level,
                Person = person,
                Role = dto.Role
            };

            duty.Membership.Add(membership);

            CommitChanges();

            //TODO: Create an event here for a new membership addition.
            
            return CreatedAtAction(nameof(CollateralDutyMembershipController.Get),
                nameof(CollateralDutyMembershipController), new {id = membership.Id},
                new DTOs.CollateralDutyMembership.Get(membership));
        }
    }
}