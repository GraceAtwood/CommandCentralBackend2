using System;
using CommandCentral.Entities;

namespace CommandCentral.Events.Args
{
    public class AccountRegistrationEventArgs : EventArgs
    {
        public AccountRegistration AccountRegistration { get; set; }

        public AccountRegistrationEventArgs(AccountRegistration a)
        {
            AccountRegistration = a;
        }
    }
}