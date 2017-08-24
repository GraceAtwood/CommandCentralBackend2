using CommandCentral.Entities;
using NHibernate.Event;

namespace CommandCentral.Framework.Data
{
    public class MyPostLoadListener : IPostLoadEventListener
    {
        public void OnPostLoad(PostLoadEvent @event)
        {
            if (@event.Entity is Person person)
            {
                for (var x = 0; x < person.PermissionGroups.Count; x++)
                {
                    person.PermissionGroups[x] = Authorization.PermissionsCache.PermissionGroupsCache[person.PermissionGroups[x].Name];
                }
            }
        }
    }
}
