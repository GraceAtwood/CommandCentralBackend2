using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.CorrespondenceItem
{
    public class Update
    {
        public Guid SubmittedFor { get; set; }
        public Guid FinalApprover { get; set; }
        public Guid Type { get; set; }
        public string Body { get; set; }
        public bool HasPhysicalCounterpart { get; set; }
    }
}
