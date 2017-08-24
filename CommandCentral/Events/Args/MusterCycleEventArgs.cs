using CommandCentral.Entities.Muster;
using System;

namespace CommandCentral.Events.Args
{
    public class MusterCycleEventArgs : EventArgs
    {
        public MusterCycle MusterCycle { get; set; }
    }
}
