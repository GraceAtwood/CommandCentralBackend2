using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.PersonProfileControllers
{
    public partial class PersonsController
    {
        [HttpGet("{personId}/AccountHistory")]
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
        public IActionResult GetAccountHistoryItem(Guid personId, Guid id)
        {
            var item = DBSession.Query<AccountHistoryEvent>().FirstOrDefault(x => x.Id == id && x.Person.Id == personId);
            if (item == null)
                return NotFound("An account history event item with that id could not be found for a person with the given id.");

            if (!User.GetFieldPermissions<Person>(item.Person).CanReturn(x => x.AccountHistory))
                return Forbid();

            return Ok(new DTOs.AccountHistoryEvent.Get(item));
        }
    }
}

