using CommandCentral.Events.Args;

namespace CommandCentral.Events.Handlers.Email
{
    public class WatchbillHandler : IEventHandler
    {
        public WatchbillHandler()
        {
            EventManager.WatchbillAssigned += OnWatchbillAssigned;
            EventManager.WatchbillPendingReview += OnWatchbillPendingReview;
            EventManager.WatchbillPublished += OnWatchbillPublished;
        }

        private void OnWatchbillPublished(object sender, WatchbillEventArgs e)
        {
            //TODO
        }

        private void OnWatchbillPendingReview(object sender, WatchbillEventArgs e)
        {
            //TODO
        }

        private void OnWatchbillAssigned(object sender, WatchbillEventArgs e)
        {
            //TODO
        }
    }
}