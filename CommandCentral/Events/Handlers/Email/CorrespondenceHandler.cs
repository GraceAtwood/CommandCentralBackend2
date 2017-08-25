using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Email;
using CommandCentral.Email.Models;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Events.Args;
using CommandCentral.Framework.Data;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace CommandCentral.Events.Handlers.Email
{
    public class CorrespondenceHandler : IEventHandler
    {
        public CorrespondenceHandler()
        {
            EventManager.CorrespondenceCompleted += OnCorrespondenceCompleted;
            EventManager.CorrespondenceCreated += OnCorrespondenceCreated;
            EventManager.CorrespondenceDeleted += OnCorrespondenceDeleted;
            EventManager.CorrespondenceModified += OnCorrespondenceModified;
            EventManager.CorrespondenceRoutedToNextPerson += OnCorrespondenceRoutedToNextPerson;
            EventManager.CorrespondenceShared += OnCorrespondenceShared;
            EventManager.ReviewDeleted += OnReviewDeleted;
            EventManager.ReviewModified += OnReviewModified;
        }

        private void OnReviewModified(object sender, CorrespondenceReviewEventArgs e)
        {
        }

        private void OnReviewDeleted(object sender, CorrespondenceReviewEventArgs e)
        {
        }

        private void OnCorrespondenceShared(object sender, CorrespondenceItemEventArgs e)
        {
        }

        private void OnCorrespondenceRoutedToNextPerson(object sender, CorrespondenceItemEventArgs e)
        {
        }

        private void OnCorrespondenceModified(object sender, CorrespondenceItemEventArgs e)
        {
        }

        private void OnCorrespondenceDeleted(object sender, CorrespondenceItemEventArgs e)
        {
        }

        private void OnCorrespondenceCreated(object sender, CorrespondenceItemEventArgs e)
        {
        }

        private void OnCorrespondenceCompleted(object sender, CorrespondenceItemEventArgs e)
        {
            var groupsWithAccessToAdminModules = PermissionsCache.PermissionGroupsCache
                .Values.Where(x => x.AccessibleSubmodules.Contains(SubModules.AdminTools))
                .Select(x => x.Name)
                .ToArray();

            var interestedPersons = new[]
                    {e.Item.FinalApprover, e.Item.SubmittedBy, e.Item.SubmittedFor}
                .Concat(e.Item.SharedWith)
                .Concat(e.Item.Reviews.Select(x => x.ReviewedBy))
                .Concat(e.Item.Reviews.Select(x => x.Reviewer))
                .Concat(e.Item.Reviews.Select(x => x.RoutedBy));
            
            using (var session = SessionManager.GetCurrentSession())
            {
                interestedPersons = interestedPersons.Concat(session.Query<Person>()
                    .Where(x => x.PermissionGroups.Any(y => y.Name.IsIn(groupsWithAccessToAdminModules)))
                    .ToFuture());

                var message = new CCEmailMessage()
                    .Subject("Correspondence Completed")
                    .HighPriority();

                foreach (var person in interestedPersons.Distinct())
                {
                    var sendToAddress = person.EmailAddresses.FirstOrDefault(x => x.IsPreferred);
                    if (sendToAddress == null)
                        continue;

                    message.To(sendToAddress)
                        .BodyFromTemplate(Templates.CorrespondenceModifiedTemplate,
                            new CorrespondenceModified(person, e.Item))
                        .Send();

                }
            }
        }
    }
}