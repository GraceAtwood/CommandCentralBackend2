using CommandCentral.Events.Args;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using CommandCentral.Controllers.CollateralDutyTrackingControllers;

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

        /// <summary>
        /// Occurs when the muster cycle is finalized either manually by a client or by a cron operation at the end of a day.
        /// </summary>
        public static event EventHandler<MusterCycleEventArgs> MusterFinalized;
        
        /// <summary>
        /// Triggers the <seealso cref="MusterFinalized"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnMusterFinalized(MusterCycleEventArgs e, object sender)
        {
            MusterFinalized?.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when a muster cycle opens after a previous muster cycle has been rolled over.
        /// </summary>
        public static event EventHandler<MusterCycleEventArgs> MusterOpened;
        
        /// <summary>
        /// Triggers the <seealso cref="MusterOpened"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnMusterOpened(MusterCycleEventArgs e, object sender)
        {
            MusterOpened?.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when a muster cycle was previously closed (finalized) but then reopened by a client for further modifications.
        /// </summary>
        public static event EventHandler<MusterCycleEventArgs> MusterReopened;
        
        /// <summary>
        /// Triggers the <seealso cref="MusterReopened"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnMusterReopened(MusterCycleEventArgs e, object sender)
        {
            MusterReopened?.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when a muster entry is deleted by a client.
        /// </summary>
        public static event EventHandler<MusterEntryEventArgs> MusterEntryDeleted;
        
        /// <summary>
        /// Triggers the <seealso cref="MusterEntryDeleted"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnMusterEntryDeleted(MusterEntryEventArgs e, object sender)
        {
            MusterEntryDeleted?.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when a client submits a new muster entry for a person.  
        /// </summary>
        public static event EventHandler<MusterEntryEventArgs> MusterEntrySubmitted;
        
        /// <summary>
        /// Triggers the <seealso cref="MusterEntrySubmitted"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnMusterEntrySubmitted(MusterEntryEventArgs e, object sender)
        {
            MusterEntrySubmitted?.Invoke(sender, e);
        }

        #endregion

        #region Account Management 

        /// <summary>
        /// Occurs when a client submits the wrong password for a given username.
        /// </summary>
        public static event EventHandler<LoginFailedEventArgs> LoginFailed;
        
        /// <summary>
        /// Triggers the <seealso cref="LoginFailed"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnLoginFailed(LoginFailedEventArgs e, object sender)
        {
            LoginFailed?.Invoke(sender, e);
        }

        #endregion

        #region Profile

        /// <summary>
        /// Occurs when a new person is created in the application.
        /// </summary>
        public static event EventHandler<PersonCreatedEventArgs> PersonCreated;
        
        /// <summary>
        /// Triggers the <seealso cref="PersonCreated"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnPersonCreated(PersonCreatedEventArgs e, object sender)
        {
            PersonCreated?.Invoke(sender, e);
        }

        #endregion

        #region Correspondence

        /// <summary>
        /// Occurs when a client creates a new correspondence but has not yet routed it to the first person in the approval chain.
        /// </summary>
        public static event EventHandler<CorrespondenceItemEventArgs> CorrespondenceCreated;
        
        /// <summary>
        /// Triggers the <seealso cref="CorrespondenceCreated"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnCorrespondenceCreated(CorrespondenceItemEventArgs e, object sender)
        {
            CorrespondenceCreated?.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when a review gets modified.  This could occur if a client were to "unrecommend" a correspondence.
        /// </summary>
        public static event EventHandler<CorrespondenceReviewEventArgs> ReviewModified;
        
        /// <summary>
        /// Triggers the <seealso cref="ReviewModified"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnReviewModified(CorrespondenceReviewEventArgs e, object sender)
        {
            ReviewModified?.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when a client deletes a review on a correspondence item.
        /// </summary>
        public static event EventHandler<CorrespondenceReviewEventArgs> ReviewDeleted;
        
        /// <summary>
        /// Triggers the <seealso cref="ReviewDeleted"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnReviewDeleted(CorrespondenceReviewEventArgs e, object sender)
        {
            ReviewDeleted?.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when a client creates a new review for a given correspondence.  In essence, this is when a correspondence item is "routed" to a new person.
        /// </summary>
        public static event EventHandler<CorrespondenceItemRoutedEventArgs> CorrespondenceRouted;
        
        /// <summary>
        /// Triggers the <seealso cref="CorrespondenceRouted"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnCorrespondenceRouted(CorrespondenceItemRoutedEventArgs e, object sender)
        {
            CorrespondenceRouted?.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when the final approver of a correspondence item submits a review for that item.
        /// </summary>
        public static event EventHandler<CorrespondenceItemEventArgs> CorrespondenceCompleted;
        
        /// <summary>
        /// Triggers the <seealso cref="CorrespondenceCompleted"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnCorrespondenceCompleted(CorrespondenceItemEventArgs e, object sender)
        {
            CorrespondenceCompleted?.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when a number of persons are added or removed from the SharedWith list for a given correspondence item.
        /// </summary>
        public static event EventHandler<CorrespondenceItemSharedEventArgs> CorrespondenceShared;
        
        /// <summary>
        /// Triggers the <seealso cref="CorrespondenceShared"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnCorrespondenceShared(CorrespondenceItemSharedEventArgs e, object sender)
        {
            CorrespondenceShared?.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when a client deletes a correspondence item.
        /// </summary>
        public static event EventHandler<CorrespondenceItemEventArgs> CorrespondenceDeleted;
        
        /// <summary>
        /// Triggers the <seealso cref="CorrespondenceDeleted"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnCorrespondenceDeleted(CorrespondenceItemEventArgs e, object sender)
        {
            CorrespondenceDeleted?.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when a client modifies a correspondence.  This refers to fields directly on a correspondence such as the body, not items in child collections. 
        /// </summary>
        public static event EventHandler<CorrespondenceItemEventArgs> CorrespondenceModified;
        
        /// <summary>
        /// Triggers the <seealso cref="CorrespondenceModified"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnCorrespondenceModified(CorrespondenceItemEventArgs e, object sender)
        {
            CorrespondenceModified?.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when a client has successfully claimed his or her account.  After this event occurs, a client has access to that account.
        /// </summary>
        public static event EventHandler<AccountRegistrationEventArgs> AccountRegistered;
        
        /// <summary>
        /// Triggers the <seealso cref="AccountRegistered"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnAccountRegistered(AccountRegistrationEventArgs e, object sender)
        {
            AccountRegistered?.Invoke(sender, e);
        }

        #endregion
        
        #region Collateral Duty Tracking

        /// <summary>
        /// Occurs when a collateral is deleted along with all of its membership.
        /// </summary>
        public static event EventHandler<CollateralDutyEventArgs> CollateralDutyDeleted;

        /// <summary>
        /// Triggers the <seealso cref="CollateralDutyDeleted"/> event.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        public static void OnCollateralDutyDeleted(CollateralDutyEventArgs e, object sender)
        {
            CollateralDutyDeleted?.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when a collateral membership is created.
        /// </summary>
        public static event EventHandler<CollateralDutyMembershipEventArgs> CollateralDutyMembershipCreated;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <exception cref="NotImplementedException"></exception>
        public static void OnCollateralDutyMembershipCreated(CollateralDutyMembershipEventArgs e, object sender)
        {
            CollateralDutyMembershipCreated?.Invoke(sender, e);
        }

        #endregion
    }
}
