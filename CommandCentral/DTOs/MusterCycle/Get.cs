using CommandCentral.Utilities.Types;
using System;

namespace CommandCentral.DTOs.MusterCycle
{
    public class Get : Put
    {
        public Guid Id { get; set; }
        public TimeRange Range { get; set; }
        public DateTime? TimeFinalized { get; set; }
        public Guid? FinalizedBy { get; set; }
        public Guid Command { get; set; }
        public bool? WasFinalizedBySystem { get; set; }

        public Get(Entities.Muster.MusterCycle cycle)
        {
            Command = cycle.Command.Id;
            FinalizedBy = cycle.FinalizedBy?.Id;
            IsFinalized = cycle.IsFinalized;
            Range = cycle.Range;
            TimeFinalized = cycle.TimeFinalized;
            WasFinalizedBySystem = cycle.WasFinalizedBySystem;
        }
    }
}
