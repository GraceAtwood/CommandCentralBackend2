using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Email;
using CommandCentral.Email.Models;
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
                    .Where(CommonQueryStrategies.GetPersonsSubscribedToEventForPersonExpression(
                        SubscribableEvents.PersonCreated, e.AccountRegistration.Person));

                var message = new CCEmailMessage()
                    .Subject("Account Registered")
                    .HighPriority();

                var sendToAddress = e.AccountRegistration.Person.EmailAddresses.FirstOrDefault();
                if (sendToAddress != null)
                {
                    message.To(sendToAddress)
                        .BodyFromTemplate(Templates.AccountRegisteredTemplate,
                            new AccountRegistered(e.AccountRegistration.Person, e.AccountRegistration))
                        .Send();
                }

                foreach (var person in interestedPersons.Distinct())
                {
                    sendToAddress = person.EmailAddresses.FirstOrDefault(x => x.IsPreferred);
                    if (sendToAddress == null)
                        continue;

                    message.To(sendToAddress)
                        .BodyFromTemplate(Templates.AccountRegisteredTemplate,
                            new AccountRegistered(person, e.AccountRegistration))
                        .Send();
                }
                
            }
        }
    }
}