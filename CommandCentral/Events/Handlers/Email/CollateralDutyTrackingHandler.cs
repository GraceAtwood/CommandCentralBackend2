using System;
using System.Linq;
using CommandCentral.Email;
using CommandCentral.Email.Models;
using CommandCentral.Entities;
using CommandCentral.Entities.CollateralDutyTracking;
using CommandCentral.Enums;
using CommandCentral.Events.Args;
using CommandCentral.Framework.Data;
using NHibernate.Linq;

namespace CommandCentral.Events.Handlers.Email
{
    public class CollateralDutyTrackingHandler : IEventHandler
    {
        public CollateralDutyTrackingHandler()
        {
            EventManager.CollateralDutyDeleted += OnCollateralDutyDeleted;
        }

        private void OnCollateralDutyDeleted(object sender, CollateralDutyEventArgs e)
        {
            var session = SessionManager.GetCurrentSession();

            var interestedPersons = e.CollateralDuty.Membership.Select(x => x.Person);
            
            var message = new CCEmailMessage()
                .Subject("Collateral Duty Deleted")
                .HighPriority();

            foreach (var person in interestedPersons.Distinct())
            {
                var sendToAddress = person.EmailAddresses.SingleOrDefault(x => x.IsPreferred);
                if (sendToAddress == null)
                    continue;

                message.To(sendToAddress)
                    .BodyFromTemplate(Templates.CollateralDeletedTemplate,
                        new CollateralDeleted(person, e.CollateralDuty))
                    .Send();
            }
        }
    }
}