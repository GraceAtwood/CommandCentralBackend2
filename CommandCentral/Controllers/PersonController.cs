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
using CommandCentral.Authorization;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class PersonController : CommandCentralController
    {
        [HttpGet("me")]
        [RequireAuthentication]
        public IActionResult Get()
        {
            return Get(null);
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid? id = null)
        {

            Person person;
            if (!id.HasValue)
                person = User;
            else
                person = DBSession.Get<Person>(id.Value);

            if (person == null)
                return NotFound();

            var perms = User.GetFieldPermissions<Person>(person);

            DTOs.GetPersonResponseDTO dto = new DTOs.GetPersonResponseDTO
            {
                ADAMSTrainingDate = perms.GetSafeReturnValue(person, x => x.ADAMSTrainingDate),
                Age = perms.GetSafeReturnValue(person, x => x.Age),
                BilletAssignment = perms.GetSafeReturnValue(person, x => x.BilletAssignment)?.Id,
                Command = perms.GetSafeReturnValue(person, x => x.Command)?.Id,
                DateOfArrival = fieldPermissions[x => x.DateOfArrival].CanReturn
                ? person.DateOfArrival : null,
                DateOfBirth = fieldPermissions[x => x.DateOfBirth].CanReturn
                ? person.DateOfBirth : null,
                DateOfDeparture = fieldPermissions[x => x.DateOfDeparture].CanReturn
                ? person.DateOfDeparture : null,
                Department = fieldPermissions[x => x.Department].CanReturn
                ? person.Department?.Id : null,
                Designation = fieldPermissions[x => x.Designation].CanReturn
                ? person.Designation?.Id : null,
                Division = fieldPermissions[x => x.Division].CanReturn
                ? person.Division?.Id : null,
                DoDId = fieldPermissions[x => x.DoDId].CanReturn
                ? person.DoDId : null,
                DutyStatus = fieldPermissions[x => x.DutyStatus].CanReturn
                ? person.DutyStatus?.Id : null,
                EAOS = fieldPermissions[x => x.EAOS].CanReturn
                ? person.EAOS : null,
                Ethnicity = fieldPermissions[x => x.Ethnicity].CanReturn
                ? person.Ethnicity?.Id : null,
                FirstName = fieldPermissions[x => x.FirstName].CanReturn
                ? person.FirstName : null,
                GTCTrainingDate = fieldPermissions[x => x.GTCTrainingDate].CanReturn
                ? person.GTCTrainingDate : null,
                HasCompletedAWARE = fieldPermissions[x => x.HasCompletedAWARE].CanReturn
                ? (bool?)person.HasCompletedAWARE : null,
                Id = fieldPermissions[x => x.Id].CanReturn
                ? person?.Id : null,
                JobTitle = fieldPermissions[x => x.JobTitle].CanReturn
                ? person.JobTitle : null,
                LastName = fieldPermissions[x => x.LastName].CanReturn
                ? person.LastName : null,
                MiddleName = fieldPermissions[x => x.MiddleName].CanReturn
                ? person.MiddleName : null,
                Paygrade = fieldPermissions[x => x.Paygrade].CanReturn
                ? person.Paygrade?.Id : null,
                PRD = fieldPermissions[x => x.PRD].CanReturn
                ? person.PRD : null,
                PrimaryNEC = fieldPermissions[x => x.PrimaryNEC].CanReturn
                ? person.PrimaryNEC?.Id : null,
                ReligiousPreference = fieldPermissions[x => x.ReligiousPreference].CanReturn
                ? person.ReligiousPreference?.Id : null,
                Sex = fieldPermissions[x => x.Sex].CanReturn
                ? person.Sex?.Id : null,
                Shift = fieldPermissions[x => x.Shift].CanReturn
                ? person.Shift : null,
                SSN = fieldPermissions[x => x.SSN].CanReturn
                ? person.SSN : null,
                Suffix = fieldPermissions[x => x.Suffix].CanReturn
                ? person.Suffix : null,
                Supervisor = fieldPermissions[x => x.Supervisor].CanReturn
                ? person.Supervisor : null,
                UIC = fieldPermissions[x => x.UIC].CanReturn
                ? person.UIC?.Id : null,
                WorkCenter = fieldPermissions[x => x.WorkCenter].CanReturn
                ? person.WorkCenter : null,
                WorkRoom = fieldPermissions[x => x.WorkRoom].CanReturn
                ? person.WorkRoom : null
            };

            return Ok(dto);
        }
    }
}

