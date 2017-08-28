using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Events.Args;
using CommandCentral.Framework.Data;
using NHibernate.Linq;

namespace CommandCentral.Events.Handlers.Email
{
    public class AccountRegistrationHandler : IEventHandler
    {
        public AccountRegistrationHandler()
        {
            EventManager.AccountRegistered += OnAccountRegistered;
        }

        private void OnAccountRegistered(object sender, AccountRegistrationEventArgs e)
        {
            var groupsWithAccessToAdminModules = PermissionsCache.PermissionGroupsCache
                .Values.Where(x => x.AccessibleSubmodules.Contains(SubModules.AdminTools))
                .Select(x => x.Name)
                .ToArray();

            using (var session = SessionManager.GetCurrentSession())
            {
                var interestedPersons = session.Query<Person>()
                    .Where(x => x.SubscribedEvents.ContainsKey(SubscribableEvents.PersonCreated))
                    .Where(x => x.SubscribedEvents[SubscribableEvents.PersonCreated] == ChainOfCommandLevels.Division &&
                                x.Division == e.AccountRegistration.Person.Division || 
                                x.SubscribedEvents[SubscribableEvents.PersonCreated] == ChainOfCommandLevels.Department &&
                                x.Department == e.AccountRegistration.Person.Department || 
                                x.SubscribedEvents[SubscribableEvents.PersonCreated] == ChainOfCommandLevels.Command &&
                                x.Command == e.AccountRegistration.Person.Command);
                
            }
        }
    }
}