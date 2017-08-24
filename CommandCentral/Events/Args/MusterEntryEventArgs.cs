using CommandCentral.Entities.Muster;
using System;

namespace CommandCentral.Events.Args
{
    public class MusterEntryEventArgs : EventArgs
    {
        public MusterEntry MusterEntry { get; set; }
    }
}
