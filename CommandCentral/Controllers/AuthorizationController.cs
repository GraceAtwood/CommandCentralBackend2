using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Authorization;
using CommandCentral.Enums;

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

            var highestlevels = User.GetHighestAccessLevels();

            var returnableFieldsAtLevel = new Dictionary<ChainOfCommandLevels, Dictionary<string, HashSet<string>>>();

            foreach (var chainOfCommandLevel in ((ChainOfCommandLevels[])Enum.GetValues(typeof(ChainOfCommandLevels))).Where(x => x != ChainOfCommandLevels.None).OrderByDescending(x => x))
            {
                var result = new Dictionary<string, HashSet<string>>();
                returnableFieldsAtLevel[chainOfCommandLevel] = result;

                foreach (var typePermission in PermissionsCache.PermissionTypesCache)
                {
                    foreach (var propertyPermissions in typePermission.Value)
                    {
                        var type = typePermission.Key;
                        var property = propertyPermissions.Key;
                        var permissionCollection = propertyPermissions.Value;

                        if (permissionCollection.LevelsRequiredToReturnForChainOfCommand.Any(level => (level.Value == ChainOfCommandLevels.None || level.Value == chainOfCommandLevel) && highestlevels[level.Key] >= level.Value))
                        {
                            if (!result.ContainsKey(type.Name))
                                result.Add(type.Name, new HashSet<string> { property.Name });
                            else
                                result[type.Name].Add(property.Name);
                        }
                    }
                }
            }

            var dto = new DTOs.Authorization.Get
            {
                AccessibleSubmodules = ((SubModules[])Enum.GetValues(typeof(SubModules))).Where(x => User.CanAccessSubmodules(x)).ToList(),
                EditablePermissionGroups = PermissionsCache.PermissionGroupsCache.Values.Where(x => User.CanEditPermissionGroups(x)).Select(x => x.ToString()).ToList(),
                FieldPermissions = PermissionsCache.PermissionTypesCache.ToDictionary(x => x.Key.Name, x =>
                {
                    dynamic desc = Activator.CreateInstance(typeof(TypePermissionsDescriptor<>).MakeGenericType(x.Key), new[] { User, person });
                    return ((IEnumerable<PropertyPermissionsDescriptor>)desc.GetAllPermissions())
                        .ToDictionary(
                            permissionsDescriptor => permissionsDescriptor.Property.Name, 
                            permissionsDescriptor => 
                                new DTOs.Authorization.Get.PropertyPermissionsDTO
                                {
                                    CanEdit = permissionsDescriptor.CanEdit,
                                    CanReturn = permissionsDescriptor.CanReturn
                                });
                }),
                HighestLevels = highestlevels,
                IsInChainOfCommand = person == null ? null : ((ChainsOfCommand[])Enum.GetValues(typeof(ChainsOfCommand))).ToDictionary(x => x, chainOfCommand => User.IsInChainOfCommand(person, chainOfCommand)),
                PermissionGroupNames = User.PermissionGroups.Select(x => x.Name).ToList(),
                PersonId = User.Id,
                PersonResolvedAgainstId = person?.Id,
                ReturnableFieldsAtLevel = returnableFieldsAtLevel.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key, y => y.Value.ToList()))
            };

            return Ok(dto);
        }
    }
}
