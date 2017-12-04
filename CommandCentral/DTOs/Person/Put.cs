using System;
using CommandCentral.Enums;

namespace CommandCentral.DTOs.Person
{
    public class Put : Post
    {
        public string MiddleName { get; set; }
        public string Suffix { get; set; }
        public int Age { get; set; }
        public Guid? Ethnicity { get; set; }
        public Guid? ReligiousPreference { get; set; }
        public string Supervisor { get; set; }
        public string WorkCenter { get; set; }
        public string WorkRoom { get; set; }
        public string Shift { get; set; }
        public string JobTitle { get; set; }
        public DateTime? EAOS { get; set; }
        public DateTime? PRD { get; set; }
        public DateTime? DateOfDeparture { get; set; }
        public BilletAssignments BilletAssignment { get; set; }
    }
}