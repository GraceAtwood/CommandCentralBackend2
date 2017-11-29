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

        public Get(Entities.Person user, Entities.Person person)
        {
            Age = user.CanReturn(person, x => x.Age) ? person.Age : default;
            BilletAssignment = user.CanReturn(person, x => x.BilletAssignment) ? person.BilletAssignment : default;
            Command = user.CanReturn(person, x => x.Division.Department.Command)
                ? person.Division.Department.Command.Id
                : default;
            DateOfArrival = user.CanReturn(person, x => x.DateOfArrival) ? person.DateOfArrival : default;
            DateOfBirth = user.CanReturn(person, x => x.DateOfBirth) ? person.DateOfBirth : default;
            DateOfDeparture = user.CanReturn(person, x => x.DateOfDeparture) ? person.DateOfDeparture : default;
            Department = user.CanReturn(person, x => x.Division.Department) ? person.Division.Department.Id : default;
            Designation = user.CanReturn(person, x => x.Designation) ? person.Designation.Id : default;
            Division = user.CanReturn(person, x => x.Division) ? person.Division.Id : default;
            DoDId = user.CanReturn(person, x => x.DoDId) ? person.DoDId : default;
            DutyStatus = user.CanReturn(person, x => x.DutyStatus) ? person.DutyStatus : default;
            EAOS = user.CanReturn(person, x => x.EAOS) ? person.EAOS : default;
            Ethnicity = user.CanReturn(person, x => x.Ethnicity) ? person.Ethnicity?.Id : default;
            FirstName = user.CanReturn(person, x => x.FirstName) ? person.FirstName : default;
            Id = user.CanReturn(person, x => x.Id) ? person.Id : default;
            JobTitle = user.CanReturn(person, x => x.JobTitle) ? person.JobTitle : default;
            LastName = user.CanReturn(person, x => x.LastName) ? person.LastName : default;
            MiddleName = user.CanReturn(person, x => x.MiddleName) ? person.MiddleName : default;
            Paygrade = user.CanReturn(person, x => x.Paygrade) ? person.Paygrade : default;
            PRD = user.CanReturn(person, x => x.PRD) ? person.PRD : default;
            ReligiousPreference = user.CanReturn(person, x => x.ReligiousPreference)
                ? person.ReligiousPreference?.Id
                : default;
            Sex = user.CanReturn(person, x => x.Sex) ? person.Sex : default;
            Shift = user.CanReturn(person, x => x.Shift) ? person.Shift : default;
            Suffix = user.CanReturn(person, x => x.Suffix) ? person.Suffix : default;
            Supervisor = user.CanReturn(person, x => x.Supervisor) ? person.Supervisor : default;
            UIC = user.CanReturn(person, x => x.UIC) ? person.UIC.Id : default;
            WorkCenter = user.CanReturn(person, x => x.WorkCenter) ? person.WorkCenter : default;
            WorkRoom = user.CanReturn(person, x => x.WorkRoom) ? person.WorkRoom : default;
        }
    }
}