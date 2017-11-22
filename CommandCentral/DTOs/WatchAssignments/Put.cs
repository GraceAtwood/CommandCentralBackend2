using System;

namespace CommandCentral.DTOs.WatchAssignments
{
    public class Put
    {
        public bool IsAcknowledged { get; set; }
        public Guid? PersonAssigned { get; set; }
    }
}