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

            var dto = new DTOs.Person.Get
            {
                ADAMSTrainingDate = perms.GetSafeReturnValue(person, x => x.ADAMSTrainingDate),
                Age = perms.GetSafeReturnValue(person, x => x.Age),
                BilletAssignment = perms.GetSafeReturnValue(person, x => x.BilletAssignment)?.Id,
                Command = perms.GetSafeReturnValue(person, x => x.Command)?.Id,
                DateOfArrival = perms.GetSafeReturnValue(person, x => x.DateOfArrival),
                DateOfBirth = perms.GetSafeReturnValue(person, x => x.DateOfBirth),
                DateOfDeparture = perms.GetSafeReturnValue(person, x => x.DateOfDeparture),
                Department = perms.GetSafeReturnValue(person, x => x.Department)?.Id,
                Designation = perms.GetSafeReturnValue(person, x => x.Designation)?.Id,
                Division = perms.GetSafeReturnValue(person, x => x.Division)?.Id,
                DoDId = perms.GetSafeReturnValue(person, x => x.DoDId),
                DutyStatus = perms.GetSafeReturnValue(person, x => x.DutyStatus)?.Id,
                EAOS = perms.GetSafeReturnValue(person, x => x.EAOS),
                Ethnicity = perms.GetSafeReturnValue(person, x => x.Ethnicity)?.Id,
                FirstName = perms.GetSafeReturnValue(person, x => x.FirstName),
                GTCTrainingDate = perms.GetSafeReturnValue(person, x => x.GTCTrainingDate),
                HasCompletedAWARE = perms.GetSafeReturnValue(person, x => x.HasCompletedAWARE),
                Id = perms.GetSafeReturnValue(person, x => x.Id),
                JobTitle = perms.GetSafeReturnValue(person, x => x.JobTitle),
                LastName = perms.GetSafeReturnValue(person, x => x.LastName),
                MiddleName = perms.GetSafeReturnValue(person, x => x.MiddleName),
                Paygrade = perms.GetSafeReturnValue(person, x => x.Paygrade)?.Id,
                PRD = perms.GetSafeReturnValue(person, x => x.PRD),
                ReligiousPreference = perms.GetSafeReturnValue(person, x => x.ReligiousPreference)?.Id,
                Sex = perms.GetSafeReturnValue(person, x => x.Sex)?.Id,
                Shift = perms.GetSafeReturnValue(person, x => x.Shift),
                SSN = perms.GetSafeReturnValue(person, x => x.SSN),
                Suffix = perms.GetSafeReturnValue(person, x => x.Suffix),
                Supervisor = perms.GetSafeReturnValue(person, x => x.Supervisor),
                UIC = perms.GetSafeReturnValue(person, x => x.UIC)?.Id,
                WorkCenter = perms.GetSafeReturnValue(person, x => x.WorkCenter),
                WorkRoom = perms.GetSafeReturnValue(person, x => x.WorkRoom)
            };

            return Ok(dto);
        }
    }
}

