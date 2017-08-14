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
using Microsoft.AspNetCore.JsonPatch;
using System.Linq.Expressions;

namespace CommandCentral.Controllers
{
    /// <summary>
    /// The person object is the central entry to a person's profile.  Permissions for each field can be attained from the /authorization controller.
    /// </summary>
    public partial class PersonsController : CommandCentralController
    {
        [HttpGet("{personId}/AccountHistory")]
        [RequireAuthentication]
        public IActionResult GetAccountHistory(Guid personId, [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var items = DBSession.Query<AccountHistoryEvent>()
                .Where(x => x.Person.Id == personId)
                .OrderByDescending(x => x.EventTime)
                .Take(limit)
                .ToList();

            if (!items.Any())
                return Ok(new List<DTOs.AccountHistoryEvent.Get>());

            if (!User.GetFieldPermissions<Person>(items.First().Person).CanReturn(x => x.AccountHistory))
                return Forbid();

            return Ok(items.Select(item => new DTOs.AccountHistoryEvent.Get(item)));
        }

        [HttpGet("{personId}/AccountHistory/{id}")]
        [RequireAuthentication]
        public IActionResult GetAccountHistoryItem(Guid personId, Guid id)
        {
            var item = DBSession.Query<AccountHistoryEvent>().Where(x => x.Id == id && x.Person.Id == personId).FirstOrDefault();
            if (item == null)
                return NotFound("An account history event item with that id could not be found for a person with the given id.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanReturn(x => x.AccountHistory))
                return Forbid();

            return Ok(new DTOs.AccountHistoryEvent.Get(item));
        }
    }
}

