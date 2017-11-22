using System;
using CommandCentral.Entities.Watchbill;

namespace CommandCentral.DTOs.WatchAssignments
{
    public class Get : Post
    {
        public Guid Id { get; set; }
        public DateTime DateAssigned { get; set; }
        public bool IsAcknowledged { get; set; }
        public Guid? AcknowledgedBy { get; set; }
        public DateTime? DateAcknowledged { get; set; }
        public int NumberOfAlertsSent { get; set; }
        public Guid AssignedBy { get; set; } 

        public Get(WatchAssignment item)
        {
            Id = item.Id;
            Watchshift = item.WatchShift.Id;
            PersonAssigned = item.PersonAssigned.Id;
            AssignedBy = item.AssignedBy.Id;
            DateAssigned = item.DateAssigned;
            IsAcknowledged = item.IsAcknowledged;
            AcknowledgedBy = item.AcknowledgedBy?.Id;
            DateAcknowledged = item.DateAcknowledged;
            NumberOfAlertsSent = item.NumberOfAlertsSent;
        }
    }
}