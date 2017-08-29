using System;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Email;
using CommandCentral.Email.Models;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Events.Args;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using LinqKit;
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
            EventManager.CorrespondenceRouted += OnCorrespondenceRouted;
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

        private void OnCorrespondenceRouted(object sender, CorrespondenceItemRoutedEventArgs e)
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

            var chainOfCommandQuery = CommonQueryStrategies.IsPersonInChainOfCommandExpression(e.Item.SubmittedFor);

            interestedPersons = interestedPersons.Concat(SessionManager.GetCurrentSession().Query<Person>()
                .AsExpandable()
                .Where(chainOfCommandQuery.NullSafeOr(x =>
                    x.PermissionGroups.Any(group => groupsWithAccessToAdminModules.Contains(group.Name))))
                .Where(CommonQueryStrategies.GetPersonsSubscribedToEventForPersonExpression(
                    SubscribableEvents.CorrespondenceRouted, e.Item.SubmittedFor))
                .ToList());

            var message = new CCEmailMessage()
                .Subject($"Correspondence #{e.Item.SeriesNumber} ICO {e.Item.SubmittedFor.ToDisplayName()}: Routed")
                .HighPriority();

            foreach (var person in interestedPersons.Distinct())
            {
                var sendToAddress = person.EmailAddresses.SingleOrDefault(x => x.IsPreferred);
                if (sendToAddress == null)
                    continue;

                message
                    .To(sendToAddress)
                    .BodyFromTemplate(Templates.CorrespondenceRoutedTemplate,
                        new CorrespondenceRouted(person, e.Item, e.NewPersonRoutedTo))
                    .Send();
            }
        }

        private void OnCorrespondenceModified(object sender, CorrespondenceItemEventArgs e)
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

            var chainOfCommandQuery = CommonQueryStrategies.IsPersonInChainOfCommandExpression(e.Item.SubmittedFor);

            interestedPersons = interestedPersons.Concat(SessionManager.GetCurrentSession().Query<Person>()
                .AsExpandable()
                .Where(chainOfCommandQuery.NullSafeOr(x =>
                    x.PermissionGroups.Any(group => groupsWithAccessToAdminModules.Contains(group.Name))))
                .Where(CommonQueryStrategies.GetPersonsSubscribedToEventForPersonExpression(
                    SubscribableEvents.CorrespondenceModified, e.Item.SubmittedFor))
                .ToList());

            var message = new CCEmailMessage()
                .Subject($"Correspondence #{e.Item.SeriesNumber} ICO {e.Item.SubmittedFor.ToDisplayName()}: Modified")
                .HighPriority();

            foreach (var person in interestedPersons.Distinct())
            {
                var sendToAddress = person.EmailAddresses.SingleOrDefault(x => x.IsPreferred);
                if (sendToAddress == null)
                    continue;

                message
                    .To(sendToAddress)
                    .BodyFromTemplate(Templates.CorrespondenceModifiedTemplate,
                        new CorrespondenceGeneric(person, e.Item))
                    .Send();
            }
        }

        private void OnCorrespondenceDeleted(object sender, CorrespondenceItemEventArgs e)
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

            var chainOfCommandQuery = CommonQueryStrategies.IsPersonInChainOfCommandExpression(e.Item.SubmittedFor);

            interestedPersons = interestedPersons.Concat(SessionManager.GetCurrentSession().Query<Person>()
                .AsExpandable()
                .Where(chainOfCommandQuery.NullSafeOr(x =>
                    x.PermissionGroups.Any(group => groupsWithAccessToAdminModules.Contains(group.Name))))
                .Where(CommonQueryStrategies.GetPersonsSubscribedToEventForPersonExpression(
                    SubscribableEvents.CorrespondenceDeleted, e.Item.SubmittedFor))
                .ToList());

            var message = new CCEmailMessage()
                .Subject($"Correspondence #{e.Item.SeriesNumber} ICO {e.Item.SubmittedFor.ToDisplayName()}: Deleted")
                .HighPriority();

            foreach (var person in interestedPersons.Distinct())
            {
                var sendToAddress = person.EmailAddresses.SingleOrDefault(x => x.IsPreferred);
                if (sendToAddress == null)
                    continue;

                message
                    .To(sendToAddress)
                    .BodyFromTemplate(Templates.CorrespondenceDeletedTemplate,
                        new CorrespondenceGeneric(person, e.Item))
                    .Send();
            }
        }

        private void OnCorrespondenceCreated(object sender, CorrespondenceItemEventArgs e)
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

            var chainOfCommandQuery = CommonQueryStrategies.IsPersonInChainOfCommandExpression(e.Item.SubmittedFor);

            interestedPersons = interestedPersons.Concat(SessionManager.GetCurrentSession().Query<Person>()
                .AsExpandable()
                .Where(chainOfCommandQuery.NullSafeOr(x =>
                    x.PermissionGroups.Any(group => groupsWithAccessToAdminModules.Contains(group.Name))))
                .Where(CommonQueryStrategies.GetPersonsSubscribedToEventForPersonExpression(
                    SubscribableEvents.CorrespondenceCreated, e.Item.SubmittedFor))
                .ToList());

            var message = new CCEmailMessage()
                .Subject($"Correspondence #{e.Item.SeriesNumber} ICO {e.Item.SubmittedFor.ToDisplayName()}: Created")
                .HighPriority();

            foreach (var person in interestedPersons.Distinct())
            {
                var sendToAddress = person.EmailAddresses.SingleOrDefault(x => x.IsPreferred);
                if (sendToAddress == null)
                    continue;

                message
                    .To(sendToAddress)
                    .BodyFromTemplate(Templates.CorrespondenceCreatedTemplate,
                        new CorrespondenceGeneric(person, e.Item))
                    .Send();
            }
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

            var chainOfCommandQuery = CommonQueryStrategies.IsPersonInChainOfCommandExpression(e.Item.SubmittedFor);

            interestedPersons = interestedPersons.Concat(SessionManager.GetCurrentSession().Query<Person>()
                .AsExpandable()
                .Where(chainOfCommandQuery.NullSafeOr(x =>
                    x.PermissionGroups.Any(group => groupsWithAccessToAdminModules.Contains(group.Name))))
                .Where(CommonQueryStrategies.GetPersonsSubscribedToEventForPersonExpression(
                    SubscribableEvents.CorrespondenceCompleted, e.Item.SubmittedFor))
                .ToList());

            var message = new CCEmailMessage()
                .Subject($"Correspondence #{e.Item.SeriesNumber} ICO {e.Item.SubmittedFor.ToDisplayName()}: Completed")
                .HighPriority();

            foreach (var person in interestedPersons.Distinct())
            {
                var sendToAddress = person.EmailAddresses.SingleOrDefault(x => x.IsPreferred);
                if (sendToAddress == null)
                    continue;

                message
                    .To(sendToAddress)
                    .BodyFromTemplate(Templates.CorrespondenceCompletedTemplate,
                        new CorrespondenceGeneric(person, e.Item))
                    .Send();
            }
        }
    }
}