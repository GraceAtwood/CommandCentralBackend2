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

            return Ok(new DTOs.Person.Get(person, perms));
        }
    }
}

