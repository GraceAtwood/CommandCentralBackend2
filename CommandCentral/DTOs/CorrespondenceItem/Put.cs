using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.CorrespondenceItem
{
    public class Put
    {
        public Guid FinalApprover { get; set; }
        public string Body { get; set; }
        public bool HasPhysicalCounterpart { get; set; }
    }
}
