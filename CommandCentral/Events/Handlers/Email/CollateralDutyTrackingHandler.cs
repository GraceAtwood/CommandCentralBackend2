using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CommandCentral.Email;
using CommandCentral.Email.Models;
using CommandCentral.Entities;
using CommandCentral.Entities.CollateralDutyTracking;
using CommandCentral.Enums;
using CommandCentral.Events.Args;
using CommandCentral.Framework.Data;
using NHibernate.Dialect.Function;
using NHibernate.Linq;
using Remotion.Linq.Clauses;

namespace CommandCentral.Events.Handlers.Email
{
    public class CollateralDutyTrackingHandler : IEventHandler
    {
        public CollateralDutyTrackingHandler()
        {
            EventManager.CollateralDutyDeleted += OnCollateralDutyDeleted;
            EventManager.CollateralDutyMembershipCreated += OnCollateralMembershipCreated;
        }

        private void OnCollateralMembershipCreated(object sender, CollateralDutyMembershipEventArgs e)
        {
            var session = SessionManager.GetCurrentSession();

            var emails = session.Query<EmailAddress>()
                .Where(x => x.Person == e.CollateralDutyMembership.Person)
                .Where(x => x.IsPreferred);

            if (e.CollateralDutyMembership.Level == CollateralLevels.Division)
            {
                emails = emails.Concat(session.Query<CollateralDutyMembership>()
                    .Where(x => x.CollateralDuty == e.CollateralDutyMembership.CollateralDuty)
                    .Where(x => x.Level == CollateralLevels.Division)
                    .Where(x => x.Person.Division == e.CollateralDutyMembership.Person.Division)
                    .Where(x => x.Role == CollateralRoles.Primary || x.Role == CollateralRoles.Secondary)
                    .SelectMany(x => x.Person.EmailAddresses)
                    .Where(x => x.IsPreferred)
                );
            }
            
            if (e.CollateralDutyMembership.Level == CollateralLevels.Department)
            {
                emails = emails.Concat(session.Query<CollateralDutyMembership>()
                    .Where(x => x.CollateralDuty == e.CollateralDutyMembership.CollateralDuty)
                    .Where(x => x.Level == CollateralLevels.Department)
                    .Where(x => x.Person.Department == e.CollateralDutyMembership.Person.Department)
                    .Where(x => x.Role == CollateralRoles.Primary || x.Role == CollateralRoles.Secondary)
                    .SelectMany(x => x.Person.EmailAddresses)
                    .Where(x => x.IsPreferred)
                );
            }
            
            if (e.CollateralDutyMembership.Level == CollateralLevels.Command)
            {
                emails = emails.Concat(session.Query<CollateralDutyMembership>()
                    .Where(x => x.CollateralDuty == e.CollateralDutyMembership.CollateralDuty)
                    .Where(x => x.Level == CollateralLevels.Command)
                    .Where(x => x.Person.Command == e.CollateralDutyMembership.Person.Command)
                    .Where(x => x.Role == CollateralRoles.Primary || x.Role == CollateralRoles.Secondary)
                    .SelectMany(x => x.Person.EmailAddresses)
                    .Where(x => x.IsPreferred)
                );
            }

            var message = new CCEmailMessage()
                .Subject("Collateral Duty Assigned")
                .HighPriority();

            foreach (var email in emails)
            {
                if (email == null)
                    continue;
                
                message.To(email)
                    .BodyFromTemplate(Templates.CollateralMembershipCreated,
                        new CollateralMembershipCreated(email.Person, e.CollateralDutyMembership))
                    .Send();
                
            }
        }

        private void OnCollateralDutyDeleted(object sender, CollateralDutyEventArgs e)
        {
            var session = SessionManager.GetCurrentSession();
            var emails = session.Query<CollateralDutyMembership>()
                .Where(x => x.CollateralDuty == e.CollateralDuty)
                .SelectMany(x => x.Person.EmailAddresses)
                .Where(x => x.IsPreferred);
            
            var message = new CCEmailMessage()
                .Subject("Collateral Duty Deleted")
                .HighPriority();

            foreach (var email in emails.Distinct())
            {
                message.To(email)
                    .BodyFromTemplate(Templates.CollateralDeletedTemplate,
                        new CollateralDeleted(email.Person, e.CollateralDuty))
                    .Send();
            }
        }
    }
}