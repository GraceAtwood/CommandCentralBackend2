using CommandCentral.Entities;
using NHibernate.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Framework.Data
{
    public class MyPostLoadListener : IPostLoadEventListener
    {
        public void OnPostLoad(PostLoadEvent @event)
        {
            if (@event.Entity is Person person)
            {
                person.PermissionGroups = person.PermissionGroups.Select(x => Authorization.PermissionsCache.PermissionGroupsCache[x.Name]).ToList();
            }
        }
    }
}
