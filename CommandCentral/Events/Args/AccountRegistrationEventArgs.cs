using System;

namespace CommandCentral.Events.Args
{
    public class AccountRegistrationEventArgs : EventArgs
    {
        public Entities.AccountRegistration AccountRegistration { get; set; }
        public string ContinueLink { get; set; }
    }
}