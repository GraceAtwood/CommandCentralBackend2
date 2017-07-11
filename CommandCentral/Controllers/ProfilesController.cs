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
            var person = DBSession.Get<Person>(id);

            if (person == null)
                return NotFound();

            var returnableFields = new Authorization.ResolvedPermissions(User, person).ReturnableFields[typeof(Person)];

            DTOs.GetPersonResponseDTO dto = new DTOs.GetPersonResponseDTO()
            {
                ADAMSTrainingDate = returnableFields.Contains(nameof(Person.AccountHistory))
                ? person.ADAMSTrainingDate : null,
                Age = returnableFields.Contains(nameof(Person.Age))
                ? (int?)person.Age : null,
                BilletAssignment = returnableFields.Contains(nameof(person.BilletAssignment))
                ? person.BilletAssignment?.Id : null,
                Command = returnableFields.Contains(nameof(person.Command))
                ? person.Command?.Id : null,
                ContactRemarks = returnableFields.Contains(nameof(Person.ContactRemarks))
                ? person.ContactRemarks : null,
                DateOfArrival = returnableFields.Contains(nameof(Person.DateOfArrival))
                ? person.DateOfArrival : null,
                DateOfBirth = returnableFields.Contains(nameof(Person.DateOfBirth))
                ? person.DateOfBirth : null,
                DateOfDeparture = returnableFields.Contains(nameof(Person.DateOfDeparture))
                ? person.DateOfDeparture : null,
                Department = returnableFields.Contains(nameof(Person.Department))
                ? person.Department?.Id : null,
                Designation = returnableFields.Contains(nameof(Person.Designation))
                ? person.Designation?.Id : null,
                Division = returnableFields.Contains(nameof(Person.Division))
                ? person.Division?.Id : null,
                DoDId = returnableFields.Contains(nameof(Person.DoDId))
                ? person.DoDId : null,
                DutyStatus = returnableFields.Contains(nameof(Person.DutyStatus))
                ? person.DutyStatus?.Id : null,
                EAOS = returnableFields.Contains(nameof(Person.EAOS))
                ? person.EAOS : null,
                EmergencyContactInstructions = returnableFields.Contains(nameof(Person.EmergencyContactInstructions))
                ? person.EmergencyContactInstructions : null,
                Ethnicity = returnableFields.Contains(nameof(Person.Ethnicity))
                ? person.Ethnicity?.Id : null,
                FirstName = returnableFields.Contains(nameof(Person.FirstName))
                ? person.FirstName : null,
                GTCTrainingDate = returnableFields.Contains(nameof(Person.GTCTrainingDate))
                ? person.GTCTrainingDate : null,
                HasCompletedAWARE = returnableFields.Contains(nameof(Person.HasCompletedAWARE))
                ? (bool?)person.HasCompletedAWARE : null,
                Id = returnableFields.Contains(nameof(Person.Id))
                ? person?.Id : null,
                JobTitle = returnableFields.Contains(nameof(Person.JobTitle))
                ? person.JobTitle : null,
                LastName = returnableFields.Contains(nameof(Person.LastName))
                ? person.LastName : null,
                MiddleName = returnableFields.Contains(nameof(Person.MiddleName))
                ? person.MiddleName : null,
                Paygrade = returnableFields.Contains(nameof(Person.Paygrade))
                ? person.Paygrade?.Id : null,
                PRD = returnableFields.Contains(nameof(Person.PRD))
                ? person.PRD : null,
                PrimaryNEC = returnableFields.Contains(nameof(Person.PrimaryNEC))
                ? person.PrimaryNEC?.Id : null,
                ReligiousPreference = returnableFields.Contains(nameof(Person.ReligiousPreference))
                ? person.ReligiousPreference?.Id : null,
                Remarks = returnableFields.Contains(nameof(Person.Remarks))
                ? person.Remarks : null,
                Sex = returnableFields.Contains(nameof(Person.Sex))
                ? person.Sex?.Id : null,
                Shift = returnableFields.Contains(nameof(Person.Shift))
                ? person.Shift : null,
                SSN = returnableFields.Contains(nameof(Person.SSN))
                ? person.SSN : null,
                Suffix = returnableFields.Contains(nameof(Person.Suffix))
                ? person.Suffix : null,
                Supervisor = returnableFields.Contains(nameof(Person.Supervisor))
                ? person.Supervisor : null,
                UIC = returnableFields.Contains(nameof(Person.UIC))
                ? person.UIC?.Id : null,
                WorkCenter = returnableFields.Contains(nameof(Person.WorkCenter))
                ? person.WorkCenter : null,
                WorkRemarks = returnableFields.Contains(nameof(Person.WorkRemarks))
                ? person.WorkRemarks : null,
                WorkRoom = returnableFields.Contains(nameof(Person.WorkRoom))
                ? person.WorkRoom : null
            };

            return Ok(dto);
        }
    }
}

