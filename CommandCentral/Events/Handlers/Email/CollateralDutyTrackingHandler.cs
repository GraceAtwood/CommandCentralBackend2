using System;
using CommandCentral.Events.Args;

namespace CommandCentral.Events.Handlers.Email
{
    public class CollateralDutyTrackingHandler : IEventHandler
    {
        public CollateralDutyTrackingHandler()
        {
            EventManager.CollateralDutyCreated += OnCollateralDutyCreated;
            EventManager.CollateralDutyDeleted += OnCollateralDutyDeleted;
        }

        private void OnCollateralDutyDeleted(object sender, CollateralDutyEventArgs collateralDutyEventArgs)
        {
            throw new NotImplementedException();
        }

        private void OnCollateralDutyCreated(object sender, CollateralDutyEventArgs collateralDutyEventArgs)
        {
            throw new NotImplementedException();
        }
    }
}