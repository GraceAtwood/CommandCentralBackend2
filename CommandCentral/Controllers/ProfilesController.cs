using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Utilities;
using CommandCentral.Framework.Data;
using CommandCentral.Entities.ReferenceLists;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class ProfilesController : CommandCentralController
    {
        // GET api/values/5
        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid id)
        {

            Person person;
            if (id == Guid.Empty)
                person = User;
            else
                person = DBSession.Get<Person>(id);

            if (person == null)
                return NotFound();

            var fieldPermissions = new Authorization.ResolvedPermissions(User, person).FieldPermissions[typeof(Person)];

            DTOs.GetPersonResponseDTO dto = new DTOs.GetPersonResponseDTO
            {
                ADAMSTrainingDate = fieldPermissions[nameof(Person.AccountHistory)].CanReturn
                ? person.ADAMSTrainingDate : null,
                Age = fieldPermissions[nameof(Person.Age)].CanReturn
                ? (int?)person.Age : null,
                BilletAssignment = fieldPermissions[nameof(person.BilletAssignment)].CanReturn
                ? person.BilletAssignment?.Id : null,
                Command = fieldPermissions[nameof(person.Command)].CanReturn
                ? person.Command?.Id : null,
                ContactRemarks = fieldPermissions[nameof(Person.ContactRemarks)].CanReturn
                ? person.ContactRemarks : null,
                DateOfArrival = fieldPermissions[nameof(Person.DateOfArrival)].CanReturn
                ? person.DateOfArrival : null,
                DateOfBirth = fieldPermissions[nameof(Person.DateOfBirth)].CanReturn
                ? person.DateOfBirth : null,
                DateOfDeparture = fieldPermissions[nameof(Person.DateOfDeparture)].CanReturn
                ? person.DateOfDeparture : null,
                Department = fieldPermissions[nameof(Person.Department)].CanReturn
                ? person.Department?.Id : null,
                Designation = fieldPermissions[nameof(Person.Designation)].CanReturn
                ? person.Designation?.Id : null,
                Division = fieldPermissions[nameof(Person.Division)].CanReturn
                ? person.Division?.Id : null,
                DoDId = fieldPermissions[nameof(Person.DoDId)].CanReturn
                ? person.DoDId : null,
                DutyStatus = fieldPermissions[nameof(Person.DutyStatus)].CanReturn
                ? person.DutyStatus?.Id : null,
                EAOS = fieldPermissions[nameof(Person.EAOS)].CanReturn
                ? person.EAOS : null,
                EmergencyContactInstructions = fieldPermissions[nameof(Person.EmergencyContactInstructions)].CanReturn
                ? person.EmergencyContactInstructions : null,
                Ethnicity = fieldPermissions[nameof(Person.Ethnicity)].CanReturn
                ? person.Ethnicity?.Id : null,
                FirstName = fieldPermissions[nameof(Person.FirstName)].CanReturn
                ? person.FirstName : null,
                GTCTrainingDate = fieldPermissions[nameof(Person.GTCTrainingDate)].CanReturn
                ? person.GTCTrainingDate : null,
                HasCompletedAWARE = fieldPermissions[nameof(Person.HasCompletedAWARE)].CanReturn
                ? (bool?)person.HasCompletedAWARE : null,
                Id = fieldPermissions[nameof(Person.Id)].CanReturn
                ? person?.Id : null,
                JobTitle = fieldPermissions[nameof(Person.JobTitle)].CanReturn
                ? person.JobTitle : null,
                LastName = fieldPermissions[nameof(Person.LastName)].CanReturn
                ? person.LastName : null,
                MiddleName = fieldPermissions[nameof(Person.MiddleName)].CanReturn
                ? person.MiddleName : null,
                Paygrade = fieldPermissions[nameof(Person.Paygrade)].CanReturn
                ? person.Paygrade?.Id : null,
                PRD = fieldPermissions[nameof(Person.PRD)].CanReturn
                ? person.PRD : null,
                PrimaryNEC = fieldPermissions[nameof(Person.PrimaryNEC)].CanReturn
                ? person.PrimaryNEC?.Id : null,
                ReligiousPreference = fieldPermissions[nameof(Person.ReligiousPreference)].CanReturn
                ? person.ReligiousPreference?.Id : null,
                Remarks = fieldPermissions[nameof(Person.Remarks)].CanReturn
                ? person.Remarks : null,
                Sex = fieldPermissions[nameof(Person.Sex)].CanReturn
                ? person.Sex?.Id : null,
                Shift = fieldPermissions[nameof(Person.Shift)].CanReturn
                ? person.Shift : null,
                SSN = fieldPermissions[nameof(Person.SSN)].CanReturn
                ? person.SSN : null,
                Suffix = fieldPermissions[nameof(Person.Suffix)].CanReturn
                ? person.Suffix : null,
                Supervisor = fieldPermissions[nameof(Person.Supervisor)].CanReturn
                ? person.Supervisor : null,
                UIC = fieldPermissions[nameof(Person.UIC)].CanReturn
                ? person.UIC?.Id : null,
                WorkCenter = fieldPermissions[nameof(Person.WorkCenter)].CanReturn
                ? person.WorkCenter : null,
                WorkRemarks = fieldPermissions[nameof(Person.WorkRemarks)].CanReturn
                ? person.WorkRemarks : null,
                WorkRoom = fieldPermissions[nameof(Person.WorkRoom)].CanReturn
                ? person.WorkRoom : null
            };

            return Ok(dto);
        }
    }
}

