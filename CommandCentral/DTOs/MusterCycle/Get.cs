using CommandCentral.Utilities.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            this.Command = cycle.Command.Id;
            this.FinalizedBy = cycle.FinalizedBy?.Id;
            this.IsFinalized = cycle.IsFinalized;
            this.Range = cycle.Range;
            this.TimeFinalized = cycle.TimeFinalized;
            this.WasFinalizedBySystem = cycle.WasFinalizedBySystem;
        }
    }
}
