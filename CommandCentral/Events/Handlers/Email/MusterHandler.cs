using CommandCentral.Events.Args;

namespace CommandCentral.Events.Handlers.Email
{
    public class MusterHandler : IEventHandler
    {
        public MusterHandler()
        {
            /*EventManager.MusterOpened += OnMusterOpened;
            EventManager.MusterFinalized += OnMusterFinalized;
            EventManager.MusterReopened += OnMusterReopened;
            EventManager.MusterEntrySubmitted += OnMusterEntrySubmitted;
            EventManager.MusterEntryDeleted += OnMusterEntryDeleted;*/
        }

        private void OnMusterOpened(object sender, MusterCycleEventArgs e)
        {
            /*var musterPermissionsGroup = PermissionsCache.PermissionGroupsCache["Muster"].;
            var interestedPerson = SessionManager.GetCurrentSession().Query<Person>()
                .Where(x => x.PermissionGroups.Any(group => group));*/
        }

        private void OnMusterFinalized(object sender, MusterCycleEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void OnMusterReopened(object sender, MusterCycleEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void OnMusterEntrySubmitted(object sender, MusterEntryEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void OnMusterEntryDeleted(object sender, MusterEntryEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}