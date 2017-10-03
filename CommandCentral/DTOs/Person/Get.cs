using CommandCentral.Authorization;
using System;
using CommandCentral.Enums;

namespace CommandCentral.DTOs.Person
{
    public class Get : Post
    {
        public Guid? Id { get; set; }
        public string MiddleName { get; set; }
        public string Suffix { get; set; }
        public int Age { get; set; }
        public Guid? Ethnicity { get; set; }
        public Guid? ReligiousPreference { get; set; }
        public Guid Department { get; set; }
        public Guid Command { get; set; }
        public string Supervisor { get; set; }
        public string WorkCenter { get; set; }
        public string WorkRoom { get; set; }
        public string Shift { get; set; }
        public string JobTitle { get; set; }
        public DateTime? EAOS { get; set; }
        public DateTime? PRD { get; set; }
        public DateTime? DateOfDeparture { get; set; }
        public BilletAssignments BilletAssignment { get; set; }

        public Get(Entities.Person person, TypePermissionsDescriptor<Entities.Person> perms)
        {
            Age = perms.GetSafeReturnValue(person, x => x.Age);
            BilletAssignment = perms.GetSafeReturnValue(person, x => x.BilletAssignment);
            Command = perms.GetSafeReturnValue(person, x => x.Command).Id;
            DateOfArrival = perms.GetSafeReturnValue(person, x => x.DateOfArrival);
            DateOfBirth = perms.GetSafeReturnValue(person, x => x.DateOfBirth);
            DateOfDeparture = perms.GetSafeReturnValue(person, x => x.DateOfDeparture);
            Department = perms.GetSafeReturnValue(person, x => x.Department).Id;
            Designation = perms.GetSafeReturnValue(person, x => x.Designation).Id;
            Division = perms.GetSafeReturnValue(person, x => x.Division).Id;
            DoDId = perms.GetSafeReturnValue(person, x => x.DoDId);
            DutyStatus = perms.GetSafeReturnValue(person, x => x.DutyStatus);
            EAOS = perms.GetSafeReturnValue(person, x => x.EAOS);
            Ethnicity = perms.GetSafeReturnValue(person, x => x.Ethnicity)?.Id;
            FirstName = perms.GetSafeReturnValue(person, x => x.FirstName);
            Id = perms.GetSafeReturnValue(person, x => x.Id);
            JobTitle = perms.GetSafeReturnValue(person, x => x.JobTitle);
            LastName = perms.GetSafeReturnValue(person, x => x.LastName);
            MiddleName = perms.GetSafeReturnValue(person, x => x.MiddleName);
            Paygrade = perms.GetSafeReturnValue(person, x => x.Paygrade);
            PRD = perms.GetSafeReturnValue(person, x => x.PRD);
            ReligiousPreference = perms.GetSafeReturnValue(person, x => x.ReligiousPreference)?.Id;
            Sex = perms.GetSafeReturnValue(person, x => x.Sex);
            Shift = perms.GetSafeReturnValue(person, x => x.Shift);
            SSN = perms.GetSafeReturnValue(person, x => x.SSN);
            Suffix = perms.GetSafeReturnValue(person, x => x.Suffix);
            Supervisor = perms.GetSafeReturnValue(person, x => x.Supervisor);
            UIC = perms.GetSafeReturnValue(person, x => x.UIC).Id;
            WorkCenter = perms.GetSafeReturnValue(person, x => x.WorkCenter);
            WorkRoom = perms.GetSafeReturnValue(person, x => x.WorkRoom);
        }
    }
}
