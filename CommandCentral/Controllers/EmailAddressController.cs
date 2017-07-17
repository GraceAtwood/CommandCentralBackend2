using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class EmailAddressController : CommandCentralController
    {
        [HttpGet("person/{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid id)
        {
            return Ok();
        }

        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
