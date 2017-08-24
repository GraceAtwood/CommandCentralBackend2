using System;
using CommandCentral.Entities;

namespace CommandCentral.Events.Args
{
    public class PersonCreatedEventArgs : EventArgs
    {
        public Person Person { get; set; }
        public Person CreatedBy { get; set; }
    }
}
