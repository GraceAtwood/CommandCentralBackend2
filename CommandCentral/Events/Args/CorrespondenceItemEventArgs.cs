using System;
using CommandCentral.Entities.Correspondence;

namespace CommandCentral.Events.Args
{
    public class CorrespondenceItemEventArgs : EventArgs
    {
        public CorrespondenceItem Item { get; set; }
    }
}
