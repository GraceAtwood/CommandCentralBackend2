using CommandCentral.Authorization.Rules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    public static  class PermissionsCache
    {
        public static ConcurrentDictionary<Type, Dictionary<PropertyInfo, PropertyPermissionsCollection>> PermissionTypesCache { get; private set; }

        public static ConcurrentDictionary<string, PermissionGroup> PermissionGroupsCache { get; private set; }

        static PermissionsCache()
        {
            PermissionTypesCache = new ConcurrentDictionary<Type, Dictionary<PropertyInfo, PropertyPermissionsCollection>>(
                Assembly.GetExecutingAssembly().GetTypes()
                    .Where(type => type.GetCustomAttribute<HasPermissionsAttribute>() != null)
                    .Select(type => new
                    {
                        Type = type,
                        Properties = type.GetProperties()
                                        .Where(prop => prop.GetCustomAttribute<HiddenFromPermissionsAttribute>() == null)
                                        .Select(prop => new
                                        {
                                            Property = prop,
                                            PropertyPermissions = new PropertyPermissionsCollection(prop)
                                        })
                    }).ToDictionary(x => x.Type, x => x.Properties.ToDictionary(y => y.Property, y => y.PropertyPermissions, new Utilities.Types.CustomPropertyInfoEqualityComparer())));
            
            PermissionGroupsCache = new ConcurrentDictionary<string, PermissionGroup>(
                PreDefs.PreDefOf<PermissionGroup>.Get().Definitions.ToDictionary(x => x.Name, x => x));
        }
    }
}
