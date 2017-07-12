using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class AuthorizationController : CommandCentralController
    {
        [HttpGet("{id}")]
        IActionResult Get(Guid id)
        {

            var person = DBSession.Get<Person>(id);

            if (person == null)
                return BadRequest("That person id was not valid.");

            var resolvedPermissions = new Authorization.ResolvedPermissions(User, person);

            return Ok(resolvedPermissions);
        }
    }
}
