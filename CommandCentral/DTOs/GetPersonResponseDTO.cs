using CommandCentral.Entities;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs
{
    public class GetPersonResponseDTO
    {

        public Guid? Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string SSN { get; set; }
        public string DoDId { get; set; }
        public string Suffix { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public Guid? Sex { get; set; }
        public Guid? Ethnicity { get; set; }
        public Guid? ReligiousPreference { get; set; }
        public Guid? Paygrade { get; set; }
        public Guid? Designation { get; set; }
        public Guid? Division { get; set; }
        public Guid? Department { get; set; }
        public Guid? Command { get; set; }
        public DateTime? GTCTrainingDate { get; set; }
        public DateTime? ADAMSTrainingDate { get; set; }
        public bool? HasCompletedAWARE { get; set; }
        public Guid? PrimaryNEC { get; set; }
        public string Supervisor { get; set; }
        public string WorkCenter { get; set; }
        public string WorkRoom { get; set; }
        public string Shift { get; set; }
        public Guid? DutyStatus { get; set; }
        public Guid? UIC { get; set; }
        public DateTime? DateOfArrival { get; set; }
        public string JobTitle { get; set; }
        public DateTime? EAOS { get; set; }
        public DateTime? PRD { get; set; }
        public DateTime? DateOfDeparture { get; set; }
        public Guid? BilletAssignment { get; set; }
    }
}
