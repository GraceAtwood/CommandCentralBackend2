using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class AuthorizationController : CommandCentralController
    {
        [HttpGet("permissions/{id}")]
        IActionResult Get(Guid id)
        {

            var person = DBSession.Get<Person>(id);

            var resolvedPermissions = new Authorization.ResolvedPermissions(User, person);

            var dto = new DTOs.ResolvedPermissionsDTO
            {
                AccessibleSubmodules = resolvedPermissions.AccessibleSubmodules.ToList(),
                EditablePermissionGroups = resolvedPermissions.EditablePermissionGroups.Select(x => x.Name).ToList(),
                FieldPermissions = resolvedPermissions.FieldPermissions.ToDictionary(x => x.Key.Name, x => x.Value),
                HighestLevels = resolvedPermissions.HighestLevels,
                IsInChainOfCommand = resolvedPermissions.IsInChainOfCommand,
                PermissionGroupNames = resolvedPermissions.PermissionGroups.Select(x => x.Name).ToList(),
                PersonId = resolvedPermissions.Person.Id,
                PersonResolvedAgainstId = resolvedPermissions.PersonResolvedAgainst.Id,
                ReturnableFieldsAtLevel = resolvedPermissions.ReturnableFieldsAtLevel
                    .ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key.Name, y => y.Value.Select(z => z.Name).ToList()))          
            };

            return Ok(dto);
        }
    }
}
