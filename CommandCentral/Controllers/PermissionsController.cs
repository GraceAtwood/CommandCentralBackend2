using System;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers
{
    /// <summary>
    /// Authorization is the method through which a client can ask questions about their permissions with respect to another person.  
    /// From authorization, a client can also learn what submodules they have access to and other things.
    /// </summary>
    public class PermissionsController : CommandCentralController
    {
        /// <summary>
        /// Gets the permissions your client has with respect to no one.  
        /// This is the best way to determine what submodules your client has access to and other things that don't require knowledge about another user.  
        /// This endpoint can also be used to determine the "best case" permissions your client has for all users regardless of who they are.  
        /// When possible, please use this endpoint as it will execute faster and is less expensive.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(DTOs.Authorization.Get))]
        public IActionResult Get()
        {
            return Ok(GetPermissions(null));
        }

        /// <summary>
        /// Gets the permissions your client has with respect to the given person.
        /// </summary>
        /// <param name="id">The id of the person with respect to whom you want to know your client's permissions.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.Authorization.Get))]
        public IActionResult Get(Guid id)
        {
            var person = DBSession.Get<Person>(id);
            if (person == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(GetPermissions(person));
        }

        private DTOs.Authorization.Get GetPermissions(Person person)
        {
            var highestlevels = User.GetHighestAccessLevels();

            var dto = new DTOs.Authorization.Get
            {
                AccessibleSubmodules = ((SpecialPermissions[]) Enum.GetValues(typeof(SpecialPermissions)))
                    .Where(x => User.SpecialPermissions.Contains(x)).ToList(),
                HighestLevels = highestlevels,
                IsInChainOfCommand =
                    person == null
                        ? null
                        : ((ChainsOfCommand[]) Enum.GetValues(typeof(ChainsOfCommand))).ToDictionary(x => x,
                            chainOfCommand => User.IsInChainOfCommand(person, chainOfCommand)),
                PersonId = User.Id,
                PersonResolvedAgainstId = person?.Id
            };

            return dto;
        }
    }
}