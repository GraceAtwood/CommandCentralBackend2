using System;
using CommandCentral.Utilities.Types;

namespace CommandCentral.DTOs.WatchShift
{
    public class Put
    {
        public string Title { get; set; }
        public TimeRange Range { get; set; }
        public Guid ShiftType { get; set; }
        
    }
}