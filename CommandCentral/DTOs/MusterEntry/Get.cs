using System;

namespace CommandCentral.DTOs.MusterEntry
{
    public class Get : Post
    {
        public Guid Id { get; set; }
        public Guid SubmittedBy { get; set; }
        public DateTime TimeSubmitted { get; set; }
        public Guid MusterCycle { get; set; }
        public Guid? StatusPeriodSetBy { get; set; }
        public MusterArchiveInformationDTO ArchiveInformation { get; set; }

        public Get(Entities.Muster.MusterEntry entry)
        {
            AccountabilityType = entry.AccountabilityType.Id;
            ArchiveInformation = entry.ArchiveInformation == null ? null :
                new MusterArchiveInformationDTO
                {
                    Command = entry.ArchiveInformation.Command,
                    Department = entry.ArchiveInformation.Department,
                    Designation = entry.ArchiveInformation.Designation,
                    Division = entry.ArchiveInformation.Division,
                    Paygrade = entry.ArchiveInformation.Paygrade,
                    UIC = entry.ArchiveInformation.UIC
                };
            Id = entry.Id;
            MusterCycle = entry.MusterCycle.Id;
            Person = entry.Person.Id;
            StatusPeriodSetBy = entry.StatusPeriodSetBy == null ? null : (Guid?)entry.StatusPeriodSetBy.Id;
            SubmittedBy = entry.SubmittedBy.Id;
            TimeSubmitted = entry.TimeSubmitted;
        }

        public class MusterArchiveInformationDTO
        {
            public string Command { get; set; }
            public string Department { get; set; }
            public string Division { get; set; }
            public string Designation { get; set; }
            public string Paygrade { get; set; }
            public string UIC { get; set; }
        }
            
    }
}
