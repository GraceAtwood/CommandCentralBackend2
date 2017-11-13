using System;
using System.Linq;
using CommandCentral.Email;
using CommandCentral.Email.Models;
using CommandCentral.Entities.CollateralDutyTracking;
using CommandCentral.Enums;
using CommandCentral.Events.Args;
using CommandCentral.Framework.Data;

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

            var query = session.Query<CollateralDutyMembership>()
                .Where(x => x.CollateralDuty == e.CollateralDutyMembership.CollateralDuty &&
                            (x.Role == CollateralRoles.Primary || x.Role == CollateralRoles.Secondary));

            switch (e.CollateralDutyMembership.Level)
            {
                case CollateralLevels.Division:
                {
                    query = query.Where(x =>
                        x.Level == CollateralLevels.Division &&
                        x.Person.Division == e.CollateralDutyMembership.Person.Division);
                    break;
                }
                case CollateralLevels.Department:
                {
                    query = query.Where(x =>
                        x.Level == CollateralLevels.Department &&
                        x.Person.Division.Department == e.CollateralDutyMembership.Person.Division.Department);
                    break;
                }
                case CollateralLevels.Command:
                {
                    query = query.Where(x =>
                        x.Level == CollateralLevels.Command &&
                        x.Person.Division.Department.Command ==
                        e.CollateralDutyMembership.Person.Division.Department.Command);
                    break;
                }
                default:
                {
                    throw new NotImplementedException(
                        $"Fell to default of switch in {nameof(OnCollateralMembershipCreated)} for value {nameof(e.CollateralDutyMembership.Level)}");
                }
            }

            var emails = query.SelectMany(x => x.Person.EmailAddresses.Where(y => y.IsPreferred)).ToList();
            var personEmail = e.CollateralDutyMembership.Person.EmailAddresses.SingleOrDefault(x => x.IsPreferred);
            if (personEmail != null)
                emails.Add(personEmail);

            var message = new CCEmailMessage()
                .Subject("Collateral Duty Assigned")
                .HighPriority();

            foreach (var email in emails)
            {
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