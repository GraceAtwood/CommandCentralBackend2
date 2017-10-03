using System;

namespace CommandCentral.DTOs.StatusPeriod
{
    public class Get : Post
    {
        public Guid Id { get; set; }
        public DateTime DateSubmitted { get; set; }
        public Guid SubmittedBy { get; set; }
        public Guid LastModifiedBy { get; set; }
        public DateTime DateLastModified { get; set; }

        public Get(Entities.Muster.StatusPeriod item)
        {
            DateSubmitted = item.DateSubmitted;
            ExemptsFromWatch = item.ExemptsFromWatch;
            Id = item.Id;
            Person = item.Person.Id;
            Range = item.Range;
            Reason = item.AccountabilityType;
            SubmittedBy = item.SubmittedBy.Id;
            DateLastModified = item.DateLastModified;
            LastModifiedBy = item.LastModifiedBy.Id;
        }
    }
}
