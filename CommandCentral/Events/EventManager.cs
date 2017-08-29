using CommandCentral.Events.Args;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace CommandCentral.Events
{
    /// <summary>
    /// Contains all of the events that can be fired in the application.
    /// </summary>
    public static class EventManager
    {
        /// <summary>
        /// Contains all of the classes that define the event handlers.
        /// </summary>
        public static ConcurrentBag<IEventHandler> EventHandlers;

        static EventManager()
        {
            EventHandlers = new ConcurrentBag<IEventHandler>(Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => typeof(IEventHandler).IsAssignableFrom(x) && x != typeof(IEventHandler))
                .Select(x => (IEventHandler) Activator.CreateInstance(x)));
        }
        
        #region Muster

        public static event EventHandler<MusterCycleEventArgs> MusterFinalized;
        public static void OnMusterFinalized(MusterCycleEventArgs e, object sender)
        {
            MusterFinalized?.Invoke(sender, e);
        }

        public static event EventHandler<MusterCycleEventArgs> MusterOpened;
        public static void OnMusterOpened(MusterCycleEventArgs e, object sender)
        {
            MusterOpened?.Invoke(sender, e);
        }

        public static event EventHandler<MusterCycleEventArgs> MusterReopened;
        public static void OnMusterReopened(MusterCycleEventArgs e, object sender)
        {
            MusterReopened?.Invoke(sender, e);
        }

        public static event EventHandler<MusterEntryEventArgs> MusterEntryDeleted;
        public static void OnMusterEntryDeleted(MusterEntryEventArgs e, object sender)
        {
            MusterEntryDeleted?.Invoke(sender, e);
        }

        public static event EventHandler<MusterEntryEventArgs> MusterEntrySubmitted;
        public static void OnMusterEntrySubmitted(MusterEntryEventArgs e, object sender)
        {
            MusterEntrySubmitted?.Invoke(sender, e);
        }

        #endregion

        #region Account Management 

        public static event EventHandler<LoginFailedEventArgs> LoginFailed;
        public static void OnLoginFailed(LoginFailedEventArgs e, object sender)
        {
            LoginFailed?.Invoke(sender, e);
        }

        public static event EventHandler<AccountRegistrationEventArgs> RegistrationStarted;
        public static void OnRegistrationStarted(AccountRegistrationEventArgs e, object sender)
        {
            RegistrationStarted?.Invoke(sender, e);
        }

        #endregion

        #region Profile

        public static event EventHandler<PersonCreatedEventArgs> PersonCreated;
        public static void OnPersonCreated(PersonCreatedEventArgs e, object sender)
        {
            PersonCreated?.Invoke(sender, e);
        }

        #endregion

        #region Correspondence

        public static event EventHandler<CorrespondenceItemEventArgs> CorrespondenceCreated;
        public static void OnCorrespondenceCreated(CorrespondenceItemEventArgs e, object sender)
        {
            CorrespondenceCreated?.Invoke(sender, e);
        }

        public static event EventHandler<CorrespondenceReviewEventArgs> NewReviewSubmitted;
        public static void OnNewReviewSubmitted(CorrespondenceReviewEventArgs e, object sender)
        {
            NewReviewSubmitted?.Invoke(sender, e);
        }

        public static event EventHandler<CorrespondenceReviewEventArgs> ReviewModified;
        public static void OnReviewModified(CorrespondenceReviewEventArgs e, object sender)
        {
            ReviewModified?.Invoke(sender, e);
        }

        public static event EventHandler<CorrespondenceReviewEventArgs> ReviewDeleted;
        public static void OnReviewDeleted(CorrespondenceReviewEventArgs e, object sender)
        {
            ReviewDeleted?.Invoke(sender, e);
        }

        public static event EventHandler<CorrespondenceItemRoutedEventArgs> CorrespondenceRouted;
        public static void OnCorrespondenceRouted(CorrespondenceItemRoutedEventArgs e, object sender)
        {
            CorrespondenceRouted?.Invoke(sender, e);
        }

        public static event EventHandler<CorrespondenceItemEventArgs> CorrespondenceCompleted;
        public static void OnCorrespondenceCompleted(CorrespondenceItemEventArgs e, object sender)
        {
            CorrespondenceCompleted?.Invoke(sender, e);
        }

        public static event EventHandler<CorrespondenceItemSharedEventArgs> CorrespondenceShared;
        public static void OnCorrespondenceShared(CorrespondenceItemSharedEventArgs e, object sender)
        {
            CorrespondenceShared?.Invoke(sender, e);
        }

        public static event EventHandler<CorrespondenceItemEventArgs> CorrespondenceDeleted;
        public static void OnCorrespondenceDeleted(CorrespondenceItemEventArgs e, object sender)
        {
            CorrespondenceDeleted?.Invoke(sender, e);
        }

        public static event EventHandler<CorrespondenceItemEventArgs> CorrespondenceModified;
        public static void OnCorrespondenceModified(CorrespondenceItemEventArgs e, object sender)
        {
            CorrespondenceModified?.Invoke(sender, e);
        }

        #endregion

    }
}
