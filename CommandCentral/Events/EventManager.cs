using CommandCentral.Events.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Events
{
    public static class EventManager
    {
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

        #endregion

        public static event EventHandler<LoginFailedEventArgs> LoginFailed;
        public static void OnLoginFailed(LoginFailedEventArgs e, object sender)
        {
            LoginFailed?.Invoke(sender, e);
        }

    }
}
