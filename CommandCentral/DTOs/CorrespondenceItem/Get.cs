using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.CorrespondenceItem
{
    public class Get : Update
    {
        public Guid Id { get; set; }
        public int SeriesNumber { get; set; }
        public Guid SubmittedFor { get; set; }
        public Guid SubmittedBy { get; set; }
        public DateTime TimeSubmitted { get; set; }
        public Guid FinalApprover { get; set; }
        public bool HasBeenCompleted { get; set; }
        public Guid Type { get; set; }
        public string Body { get; set; }
        public bool HasPhysicalCounterpart { get; set; }

        public Get(Entities.Correspondence.CorrespondenceItem item)
        {
            Id = item.Id;
            SeriesNumber = item.SeriesNumber;
            SubmittedFor = item.SubmittedFor.Id;
            SubmittedBy = item.SubmittedBy.Id;
            TimeSubmitted = item.TimeSubmitted;
            FinalApprover = item.FinalApprover.Id;
            HasBeenCompleted = item.HasBeenCompleted;
            Type = item.Type.Id;
            Body = item.Body;
            HasPhysicalCounterpart = item.HasPhysicalCounterpart;
        }
    }
}
