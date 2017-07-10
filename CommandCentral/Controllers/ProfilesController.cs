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

            var resolvedPermissions = User.ResolvePermissions(person);

            var returnableFields = new HashSet<string>(resolvedPermissions.ReturnableFields[nameof(Person)]);

            DTOs.GetPersonResponseDTO dto = new DTOs.GetPersonResponseDTO();
            dto.AccountHistory = returnableFields.Contains(nameof(Person.AccountHistory))
                ? person.AccountHistory.Select(x => x.Id) : null;
            dto.ADAMSTrainingDate = returnableFields.Contains(nameof(Person.AccountHistory))
                ? person.ADAMSTrainingDate : null;
            dto.Age = returnableFields.Contains(nameof(Person.Age))
                ? (int?)person.Age : null;
            dto.BilletAssignment = returnableFields.Contains(nameof(person.BilletAssignment))
                ? person.BilletAssignment?.Id : null;
            dto.Changes = returnableFields.Contains(nameof(person.Changes))
                ? person.Changes?.Select(x => x.Id) : null;
            dto.Command = returnableFields.Contains(nameof(person.Command))
                ? person.Command?.Id : null;
            dto.Comments = returnableFields.Contains(nameof(Person.Comments))
                ? person.Comments?.Select(x => x.Id) : null;
            dto.ContactRemarks = returnableFields.Contains(nameof(Person.ContactRemarks))
                ? person.ContactRemarks : null;
            dto.DateOfArrival = returnableFields.Contains(nameof(Person.DateOfArrival))
                ? person.DateOfArrival : null;
            dto.DateOfBirth = returnableFields.Contains(nameof(Person.DateOfBirth))
                ? person.DateOfBirth : null;
            dto.DateOfDeparture = returnableFields.Contains(nameof(Person.DateOfDeparture))
                ? person.DateOfDeparture : null;
            dto.Department = returnableFields.Contains(nameof(Person.Department))
                ? person.Department?.Id : null;
            dto.Designation = returnableFields.Contains(nameof(Person.Designation))
                ? person.Designation?.Id : null;
            dto.Division = returnableFields.Contains(nameof(Person.Division))
                ? person.Division?.Id : null;
            dto.DoDId = returnableFields.Contains(nameof(Person.DoDId))
                ? person.DoDId : null;
            dto.DutyStatus = returnableFields.Contains(nameof(Person.DutyStatus))
                ? person.DutyStatus?.Id : null;
            dto.EAOS = returnableFields.Contains(nameof(Person.EAOS))
                ? person.EAOS : null;
            dto.EmailAddresses = returnableFields.Contains(nameof(Person.EmailAddresses))
                ? person.EmailAddresses?.Select(x => x.Id) : null;
            dto.EmergencyContactInstructions = returnableFields.Contains(nameof(Person.EmergencyContactInstructions))
                ? person.EmergencyContactInstructions : null;
            dto.Ethnicity = returnableFields.Contains(nameof(Person.Ethnicity))
                ? person.Ethnicity?.Id : null;
            dto.FirstName = returnableFields.Contains(nameof(Person.FirstName))
                ? person.FirstName : null;
            dto.GTCTrainingDate = returnableFields.Contains(nameof(Person.GTCTrainingDate))
                ? person.GTCTrainingDate : null;
            dto.HasCompletedAWARE = returnableFields.Contains(nameof(Person.HasCompletedAWARE))
                ? (bool?)person.HasCompletedAWARE : null;
            dto.Id = returnableFields.Contains(nameof(Person.Id))
                ? person?.Id : null;
            dto.JobTitle = returnableFields.Contains(nameof(Person.JobTitle))
                ? person.JobTitle : null;
            dto.LastName = returnableFields.Contains(nameof(Person.LastName))
                ? person.LastName : null;
            dto.MiddleName = returnableFields.Contains(nameof(Person.MiddleName))
                ? person.MiddleName : null;
            dto.Paygrade = returnableFields.Contains(nameof(Person.Paygrade))
                ? person.Paygrade?.Id : null;
            dto.PermissionGroupNames = returnableFields.Contains(nameof(Person.PermissionGroupNames))
                ? person.PermissionGroupNames : null;
            dto.PhoneNumbers = returnableFields.Contains(nameof(Person.PhoneNumbers))
                ? resolvedPermissions.IsInChainOfCommand.Any(x => x.Value) 
                    ? person.PhoneNumbers?.Select(x => x.Id) 
                    : person.PhoneNumbers.Where(x => x.PhoneType == ReferenceListHelper<PhoneNumberType>.Find("Work")).Select(x => x.Id) 
                : null;
            dto.PhysicalAddresses = returnableFields.Contains(nameof(Person.PhysicalAddresses))
                ? person.PhysicalAddresses?.Select(x => x.Id) : null;
            dto.PRD = returnableFields.Contains(nameof(Person.PRD))
                ? person.PRD : null;
            dto.PrimaryNEC = returnableFields.Contains(nameof(Person.PrimaryNEC))
                ? person.PrimaryNEC?.Id : null;
            dto.ReligiousPreference = returnableFields.Contains(nameof(Person.ReligiousPreference))
                ? person.ReligiousPreference?.Id : null;
            dto.Remarks = returnableFields.Contains(nameof(Person.Remarks))
                ? person.Remarks : null;
            dto.SecondaryNECs = returnableFields.Contains(nameof(Person.SecondaryNECs))
                ? person.SecondaryNECs?.Select(x => x.Id) : null;
            dto.Sex = returnableFields.Contains(nameof(Person.Sex))
                ? person.Sex?.Id : null;
            dto.Shift = returnableFields.Contains(nameof(Person.Shift))
                ? person.Shift : null;
            dto.SSN = returnableFields.Contains(nameof(Person.SSN))
                ? person.SSN : null;
            dto.SubscribedEvents = returnableFields.Contains(nameof(Person.SubscribedEvents))
                ? person.SubscribedEvents.ToDictionary(x => x.Key, x => x.Value) : null;
            dto.Suffix = returnableFields.Contains(nameof(Person.Suffix))
                ? person.Suffix : null;
            dto.Supervisor = returnableFields.Contains(nameof(Person.Supervisor))
                ? person.Supervisor : null;
            dto.UIC = returnableFields.Contains(nameof(Person.UIC))
                ? person.UIC?.Id : null;
            dto.WatchAssignments = returnableFields.Contains(nameof(Person.WatchAssignments))
                ? person.WatchAssignments?.Select(x => x.Id) : null;
            dto.WatchQualifications = returnableFields.Contains(nameof(Person.WatchQualifications))
                ? person.WatchQualifications?.Select(x => x.Id) : null;
            dto.WorkCenter = returnableFields.Contains(nameof(Person.WorkCenter))
                ? person.WorkCenter : null;
            dto.WorkRemarks = returnableFields.Contains(nameof(Person.WorkRemarks))
                ? person.WorkRemarks : null;
            dto.WorkRoom = returnableFields.Contains(nameof(Person.WorkRoom))
                ? person.WorkRoom : null;
            dto.WorkRoom = "fuck this shit";

            return Ok(dto);
        }
    }
}

