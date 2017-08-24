using CommandCentral.Entities;
using System;

namespace CommandCentral.Events.Args
{
    public class LoginFailedEventArgs : EventArgs
    {
        public Person Person { get; set; }
    }
}
