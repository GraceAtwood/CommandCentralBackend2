using CommandCentral.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.Person
{
    public class Query
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string SSN { get; set; }
        public string DoDId { get; set; }
        public string Designation { get; set; }
        public string NECs { get; set; }
        public string DutyStatus { get; set; }
        public string UIC { get; set; }
        public string Sex { get; set; }
        public string Ethnicity { get; set; }
        public string ReligiousPreference { get; set; }
        public string Department { get; set; }
        public string Command { get; set; }
        public string Division { get; set; }
        public string Supervisor { get; set; }
        public string WorkCenter { get; set; }
        public string WorkRoom { get; set; }
        public string Shift { get; set; }
        public string JobTitle { get; set; }
        public string WatchQualifications { get; set; }
        public string BilletAssignment { get; set; }
        public string PermissionGroups { get; set; }
        public DateTimeRangeQuery EAOS { get; set; }
        public DateTimeRangeQuery PRD { get; set; }
        public DateTimeRangeQuery DateOfDeparture { get; set; }
        public DateTimeRangeQuery DateOfBirth { get; set; }
        public DateTimeRangeQuery DateOfArrival { get; set; }
        public Dictionary<ChangeEvents, ChainOfCommandLevels> SubscribedEvents { get; set; }
    }
}
