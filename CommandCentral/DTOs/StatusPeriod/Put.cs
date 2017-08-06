using CommandCentral.Utilities.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.StatusPeriod
{
    public class Put
    {
        public Guid Reason { get; set; }
        public TimeRange Range { get; set; }
        public bool ExemptsFromWatch { get; set; }
    }
}
