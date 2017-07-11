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
        public static ConcurrentDictionary<Type, HashSet<PropertyPermissionsCollection>> PermissionTypesCache { get; private set; }

        public static ConcurrentDictionary<string, PermissionGroup> PermissionGroupsCache { get; private set; }

        static PermissionsCache()
        {
            PermissionTypesCache = new ConcurrentDictionary<Type, HashSet<PropertyPermissionsCollection>>(
                Assembly.GetExecutingAssembly().GetTypes()
                    .Where(type => type.GetCustomAttribute<HasPermissionsAttribute>() != null)
                    .Select(type => new KeyValuePair<Type, HashSet<PropertyPermissionsCollection>>(type,
                        new HashSet<PropertyPermissionsCollection>(type.GetProperties()
                            .Where(prop => prop.GetCustomAttribute<HiddenFromPermissionsAttribute>() == null)
                            .Select(prop => new PropertyPermissionsCollection(prop)).ToList())))
                    .ToDictionary(x => x.Key, x => x.Value));

            PermissionGroupsCache = new ConcurrentDictionary<string, PermissionGroup>(
                PreDefs.PreDefOf<PermissionGroup>.Get().Definitions.ToDictionary(x => x.Name, x => x));
                    
        }

    }
}
