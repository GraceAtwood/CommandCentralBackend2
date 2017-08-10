using CommandCentral.Utilities.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.MusterCycle
{
    public class Get : Patch
    {
        public Guid Id { get; set; }
        public TimeRange Range { get; set; }
        public DateTime? TimeFinalized { get; set; }
        public Guid? FinalizedBy { get; set; }
        public Guid Command { get; set; }
    }
}
