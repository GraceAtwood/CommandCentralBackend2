using System;

namespace CommandCentral.DTOs.CorrespondenceItem
{
    public class Put
    {
        public Guid FinalApprover { get; set; }
        public string Body { get; set; }
        public bool HasPhysicalCounterpart { get; set; }
    }
}
