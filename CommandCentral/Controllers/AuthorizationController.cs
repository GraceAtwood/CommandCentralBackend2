using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Authorization;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class AuthorizationController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        public IActionResult Get()
        {
            return Get(null);
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid? id = null)
        {
            Person person = null;
            if (id.HasValue)
                person = DBSession.Get<Person>(id.Value);

            var dto = new DTOs.ResolvedPermissionsDTO
            {
                AccessibleSubmodules = ((Enums.SubModules[])Enum.GetValues(typeof(Enums.SubModules))).Where(x => User.CanAccessSubmodules(new Enums.SubModules[] { x })).ToList(),
                EditablePermissionGroups = PermissionsCache.PermissionGroupsCache.Values.Where(x => User.CanEditPermissionGroups(new PermissionGroup[] { x })).Select(x => x.ToString()).ToList(),
                FieldPermissions = new Dictionary<string, Dictionary<string, PropertyPermissionsDescriptor>> { { nameof(Person), new TypePermissionsDescriptor<Person>(User, person).GetAllPermissions().ToDictionary(x => x.Property.Name, x => x)  } },
                HighestLevels = User.GetHighestAccessLevels(),
                IsInChainOfCommand = ((Enums.ChainsOfCommand[])Enum.GetValues(typeof(Enums.ChainsOfCommand))).ToDictionary(x=> x, x=> User.IsInChainOfCommand(person, new Enums.ChainsOfCommand[] { x })),
                PermissionGroupNames = User.PermissionGroups.Select(x => x.Name).ToList(),
                PersonId = User.Id,
                PersonResolvedAgainstId = person.Id,
                ReturnableFieldsAtLevel = resolvedPermissions.ReturnableFieldsAtLevel // TODO: Fix this
                    .ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key.Name, y => y.Value.Select(z => z.Name).ToList()))          
            };

            return Ok(dto);
        }
    }
}
