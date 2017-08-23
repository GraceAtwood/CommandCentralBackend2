using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.AccountManagementControllers
{
    /// <summary>
    /// Authorization is the method through which a client can ask questions about their permissions with respect to another person.  
    /// From authorization, a client can also learn what submodules they have access to and other things.
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class AuthorizationController : CommandCentralController
    {
        /// <summary>
        /// Gets the permissions your client has with respect to no one.  
        /// This is the best way to determine what submodules your client has access to and other things that don't require knowledge about another user.  
        /// This endpoint can also be used to determine the "best case" permissions your client has for all users regardless of who they are.  
        /// When possible, please use this endpoint as it will execute faster and is less expensive.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [RequireAuthentication]
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
        [RequireAuthentication]
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
                    dynamic desc = Activator.CreateInstance(typeof(TypePermissionsDescriptor<>).MakeGenericType(x.Key), User, person);
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

            return dto;
        }
    }
}
