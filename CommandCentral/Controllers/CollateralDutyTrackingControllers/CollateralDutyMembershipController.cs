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
    /// <summary>
    /// Provides access to individual collateral duty memberships and query capability against all memberships.
    /// </summary>
    public class CollateralDutyMembershipController : CommandCentralController
    {
        /// <summary>
        /// Queries all coll duty memberships.
        /// </summary>
        /// <param name="level">An exact enum query for the level of a membership.</param>
        /// <param name="role">An exact enum query for the role of a membership.</param>
        /// <param name="person">A person query for the person of a membership.</param>
        /// <param name="hasDesignationLetter">A boolean query for whether or not a membership has an associated designation letter.</param>
        /// <param name="collateralDuty">A string query for the name of the collateral duty associated with a membership.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.CollateralDutyMembership.Get>))]
        public IActionResult Get([FromQuery] string level, [FromQuery] string role, [FromQuery] string person,
            [FromQuery] bool? hasDesignationLetter, [FromQuery] string collateralDuty)
        {
            var predicate = ((Expression<Func<CollateralDutyMembership, bool>>) null)
                .AddExactEnumQueryExpression(x => x.Level, level)
                .AddExactEnumQueryExpression(x => x.Role, role)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddNullableBoolQueryExpression(x => x.HasDesignationLetter, hasDesignationLetter);

            //Add a null safe AND phrase containing a disjunction for the name of the collateral duty or the id.
            if (!String.IsNullOrWhiteSpace(collateralDuty))
            {
                var subPredicate = collateralDuty.SplitByOr()
                    .Select(phrase =>
                    {
                        if (Guid.TryParse(phrase, out var id))
                            return x => x.Id == id;

                        return phrase.SplitByAnd()
                            .Aggregate((Expression<Func<CollateralDutyMembership, bool>>) null,
                                (current, term) => current.NullSafeAnd(x =>
                                    x.CollateralDuty.Name.Contains(term)));
                    })
                    .Aggregate<Expression<Func<CollateralDutyMembership, bool>>,
                        Expression<Func<CollateralDutyMembership, bool>>>(null,
                        (current, sub) => current.NullSafeOr(sub));

                predicate = predicate.NullSafeAnd(subPredicate);
            }

            var results = DBSession.Query<CollateralDutyMembership>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToList()
                .Select(x => new DTOs.CollateralDutyMembership.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Retrieves a single collateral duty membership.
        /// </summary>
        /// <param name="id">The Id of the membership to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.CollateralDutyMembership.Get))]
        public IActionResult Get(Guid id)
        {
            var membership = DBSession.Get<CollateralDutyMembership>(id);
            if (membership == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.CollateralDutyMembership.Get(membership));
        }

        /// <summary>
        /// Creates a new membership.
        /// The client must have access to the admin tools or have a membership in the identified coll duty in the 
        /// Primary or Secondary role at a level equal to or greater than the level at which the client wants to create the membership.
        /// </summary>
        /// <param name="dto">A dto containing the information needed to create a new membership.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.CollateralDutyMembership.Get))]
        public IActionResult Post([FromBody] DTOs.CollateralDutyMembership.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var duty = DBSession.Get<CollateralDuty>(dto.CollateralDuty);
            if (duty == null)
                return NotFoundParameter(dto.CollateralDuty, nameof(dto.CollateralDuty));

            if (!User.CanAccessSubmodules(SubModules.AdminTools))
            {
                var clientMembership = DBSession.Query<CollateralDutyMembership>().SingleOrDefault(x =>
                    x.CollateralDuty.Id == dto.CollateralDuty && x.Person == User &&
                    (x.Role == CollateralRoles.Primary || x.Role == CollateralRoles.Secondary));

                if (clientMembership == null)
                    return Forbid(
                        "In order to modify the membership of a collateral duty, you must either have access to " +
                        "the admin tools or be in the Primary or Secondary level of the collateral duty in question.");

                if (dto.Level > clientMembership.Level)
                    return Forbid(
                        "In order to add a person to a collateral duty at a given level (Division, Department, or Command)," +
                        " your level in that collateral duty must be equal to or greater than that level.  Your level is " +
                        $"{clientMembership.Level} and the level you tried to add at was {dto.Level}.");
            }

            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFoundParameter(dto.Person, nameof(dto.Person));

            if (DBSession.Query<CollateralDutyMembership>().Count(x => x.Person == person) != 0)
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

            var result = membership.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(membership);

            CommitChanges();

            EventManager.OnCollateralDutyMembershipCreated(new CollateralDutyMembershipEventArgs
            {
                CollateralDutyMembership = membership
            }, this);
            
            return CreatedAtAction(nameof(Get), new {id = membership.Id},
                new DTOs.CollateralDutyMembership.Get(membership));
        }

        /// <summary>
        /// Modifies a single membership.
        /// </summary>
        /// <param name="id">The id of the membership to modify.</param>
        /// <param name="dto">A dto containing the data needed to modify a membership.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.CollateralDutyMembership.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.CollateralDutyMembership.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var membership = DBSession.Get<CollateralDutyMembership>(id);
            if (membership == null)
                return NotFoundParameter(id, nameof(id));

            var clientMembership = DBSession.Query<CollateralDutyMembership>().SingleOrDefault(x =>
                x.CollateralDuty == membership.CollateralDuty && x.Person == User &&
                (x.Role == CollateralRoles.Primary || x.Role == CollateralRoles.Secondary));

            if (!User.CanAccessSubmodules(SubModules.AdminTools) || clientMembership == null)
                return Forbid(
                    "In order to modify the membership of a collateral duty, you must either have access to " +
                    "the admin tools or be in the Primary or Secondary level of the collateral duty in question.");

            if (dto.Level > clientMembership.Level)
                return Forbid(
                    "In order to add a person to a collateral duty at a given level (Division, Department, or Command)," +
                    " your level in that collateral duty must be equal to or greater than that level.  Your level is " +
                    $"{clientMembership.Level} and the level you tried to add at was {dto.Level}.");

            membership.Level = dto.Level;
            membership.Role = dto.Role;
            
            var result = membership.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            CommitChanges();

            //TODO: Add an event for membership modified.

            return CreatedAtAction(nameof(Get), new {id = membership.Id},
                new DTOs.CollateralDutyMembership.Get(membership));
        }

        /// <summary>
        /// Deletes a single membership.
        /// </summary>
        /// <param name="id">The id of the membership to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var membership = DBSession.Get<CollateralDutyMembership>(id);
            if (membership == null)
                return NotFoundParameter(id, nameof(id));

            var clientMembership = DBSession.Query<CollateralDutyMembership>().SingleOrDefault(x =>
                x.CollateralDuty == membership.CollateralDuty && x.Person == User &&
                (x.Role == CollateralRoles.Primary || x.Role == CollateralRoles.Secondary));

            if (!User.CanAccessSubmodules(SubModules.AdminTools) || clientMembership == null)
                return Forbid(
                    "In order to modify the membership of a collateral duty, you must either have access to " +
                    "the admin tools or be in the Primary or Secondary level of the collateral duty in question.");

            DBSession.Delete(membership);

            //TODO: Add an event here for membership modified.

            CommitChanges();

            return NoContent();
        }
    }
}