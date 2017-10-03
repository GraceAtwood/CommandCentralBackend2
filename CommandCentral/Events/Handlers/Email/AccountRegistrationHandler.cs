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

    /// <summary>
    /// Defines event handlers for registration related events. 
    /// </summary>
    public class AccountRegistrationHandler : IEventHandler
    {
        /// <summary>
        /// Registers the events.
        /// </summary>
        public AccountRegistrationHandler()
        {
            EventManager.AccountRegistered += OnAccountRegistered;
        }

        private void OnAccountRegistered(object sender, AccountRegistrationEventArgs e)
        {
            var session = SessionManager.GetCurrentSession();

            var interestedPersons = session.Query<Person>()
                .Where(CommonQueryStrategies.GetPersonsSubscribedToEventForPersonExpression(
                    SubscribableEvents.AccountRegistered, e.AccountRegistration.Person));

            var message = new CCEmailMessage()
                .Subject("Account Registered")
                .HighPriority();

            var sendToAddress = e.AccountRegistration.Person.EmailAddresses.FirstOrDefault(x => x.IsDoDEmailAddress());
            if (sendToAddress != null)
            {
                var accountRegistered = new AccountRegistered(e.AccountRegistration.Person, e.AccountRegistration);
                message.To(sendToAddress)
                    .BodyFromTemplate(Templates.AccountRegisteredTemplate,
                        accountRegistered)
                    .Send();
            }

            foreach (var person in interestedPersons.Distinct())
            {
                sendToAddress = person.EmailAddresses.SingleOrDefault(x => x.IsPreferred);
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