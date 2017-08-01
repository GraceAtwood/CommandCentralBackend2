using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Authentication;
using CommandCentral.Authorization;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class AccountHistoryEventController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        public IActionResult GetByPerson([FromQuery]Guid Person)
        {
            if (Person == Guid.Empty)
                return BadRequest("Query string with a person id is missing or malformed. Loading all phone numbers is not allowed.");

            var items = DBSession.QueryOver<AccountHistoryEvent>().Where(x => x.Person.Id == Person).List();

            if (!items.Any())
                return NotFound();

            if (!User.GetFieldPermissions<Person>(items.First().Person).CanReturn(x => x.AccountHistory))
                return Forbid();

            return Ok(items.Select(x =>
                new DTOs.AccountHistoryEvent.Get
                {
                    Id = x.Id,
                    Person = x.Person.Id,
                    AccountHistoryEventType = x.AccountHistoryEventType.Id,
                    EventTime = x.EventTime
                })
            );
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<AccountHistoryEvent>(id);
            if (item == null)
                return NotFound();

            if (!User.GetFieldPermissions<Person>(item.Person).CanReturn(x => x.AccountHistory))
                return Forbid();

            return Ok(new DTOs.AccountHistoryEvent.Get
            {
                Id = item.Id,
                Person = item.Person.Id,
                AccountHistoryEventType = item.AccountHistoryEventType.Id,
                EventTime = item.EventTime
            });
        }
    }
}
