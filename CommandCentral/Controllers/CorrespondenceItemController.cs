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
using CommandCentral.Enums;
using NHibernate.Linq;
using CommandCentral.Entities.Correspondence;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CorrespondenceItemController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.CorrespondenceItem.Get>))]
        public IActionResult Get()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.CorrespondenceItem.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<CorrespondenceItem>(id);
            if (item == null)
                return NotFound();

            if (item.SubmittedBy != User && item.SubmittedFor != User &&
                !item.Reviews.Any(x => x.Reviewer == User || x.ReviewedBy == User) &&
                !item.SharedWith.Contains(User))
                return Forbid();

            return Ok(new DTOs.CorrespondenceItem.Get(item));
        }

        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.CorrespondenceItem.Get))]
        public IActionResult Post([FromBody]DTOs.CorrespondenceItem.Update dto)
        {
            throw new NotImplementedException();
        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.CorrespondenceItem.Get))]
        public IActionResult Put(Guid id, [FromBody]DTOs.CorrespondenceItem.Update dto)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200)]
        public IActionResult Delete(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
