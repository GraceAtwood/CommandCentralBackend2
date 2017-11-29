using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.DTOs;
using CommandCentral.Entities;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.PersonProfileControllers
{
    public class AccountHistoryEventsController : CommandCentralController
    {
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.AccountHistoryEvent.Get>))]
        public IActionResult Get([FromQuery] string person, [FromQuery] DateTimeRangeQuery eventTime,
            [FromQuery] string accountHistoryEventType, [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var predicate = ((Expression<Func<AccountHistoryEvent, bool>>) null)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddDateTimeQueryExpression(x => x.EventTime, eventTime)
                .AddExactEnumQueryExpression(x => x.AccountHistoryEventType, accountHistoryEventType);

            var results = DBSession.Query<AccountHistoryEvent>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .Take(limit)
                .OrderBy(x => x.EventTime)
                .ToList()
                .Where(x => User.CanReturn(x))
                .Select(x => new DTOs.AccountHistoryEvent.Get(x))
                .ToList();

            return Ok(results);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.AccountHistoryEvent.Get))]
        public IActionResult Get(Guid id)
        {
            var accountHistoryEvent = DBSession.Get<AccountHistoryEvent>(id);
            if (accountHistoryEvent == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanReturn(accountHistoryEvent))
                return Forbid("You can't see that account history event.");

            return Ok(new DTOs.AccountHistoryEvent.Get(accountHistoryEvent));
        }
    }
}