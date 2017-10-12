using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.DTOs;
using CommandCentral.Entities;
using CommandCentral.Entities.CFS;
using CommandCentral.Enums;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities.Types;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.CFS
{
    public class CFSRequestsController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.CFSRequest.Get>))]
        public IActionResult Get([FromQuery] bool? isClaimed, [FromQuery] string person,
            [FromQuery] DateTimeRangeQuery timeSubmitted, [FromQuery] string requestType,
            [FromQuery] string claimedBy, [FromQuery] int limit = 1000)
        {
            if (!User.GetHighestAccessLevels().TryGetValue(ChainsOfCommand.CommandFinancialSpecialist, out var level)
                || level == ChainOfCommandLevels.None)
                return Forbid("You must be in the Command Financial Specialist chain of command.");

            var predicate = ((Expression<Func<Request, bool>>) null)
                .AddIsPersonInChainOfCommandExpression(x => x.Person, User, ChainsOfCommand.CommandFinancialSpecialist)
                .AddNullableBoolQueryExpression(x => x.IsClaimed, isClaimed)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddDateTimeQueryExpression(x => x.TimeSubmitted, timeSubmitted)
                .AddReferenceListQueryExpression(x => x.RequestType, requestType)
                .AddPersonQueryExpression(x => x.ClaimedBy, claimedBy);

            var results = DBSession.Query<Request>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .OrderByDescending(x => x.TimeSubmitted)
                .ToList()
                .Select(x => new DTOs.CFSRequest.Get(x))
                .ToList();

            return Ok(results);
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.CFSRequest.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<Request>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.IsInChainOfCommand(item.Person, ChainsOfCommand.CommandFinancialSpecialist))
                return Forbid("You must be in the CFS chain of command to view this request.");

            return Ok(new DTOs.CFSRequest.Get(item));
        }

        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.CFSRequest.Post))]
        public IActionResult Post([FromBody] DTOs.CFSRequest.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var person = DBSession.Get<Person>(dto.Person);
            if (person == null)
                return NotFoundParameter(dto.Person, nameof(dto.Person));

            if (User != person && !User.IsInChainOfCommand(person, ChainsOfCommand.CommandFinancialSpecialist))
                return Forbid("You must be in the CFS chain of command for this person.");

            var item = new Request
            {
                Id = Guid.NewGuid(),
                TimeSubmitted = CallTime,
                RequestType = dto.RequestType,
                Person = person
            };

            DBSession.Save(item);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.CFSRequest.Get(item));
        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.CFSRequest.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.CFSRequest.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var item = DBSession.Get<Request>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.IsInChainOfCommand(item.Person, ChainsOfCommand.CommandFinancialSpecialist))
                return Forbid("You must be in the CFS chain of command for this person.");

            var claimedBy = DBSession.Get<Person>(dto.ClaimedBy);
            if (claimedBy == null)
                return NotFoundParameter(dto.ClaimedBy, nameof(dto.ClaimedBy));

            item.ClaimedBy = claimedBy;
            item.IsClaimed = true;

            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.CFSRequest.Get(item));
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var item = DBSession.Get<Request>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.IsInChainOfCommand(item.Person, ChainsOfCommand.CommandFinancialSpecialist))
                return Forbid("You must be in this person's CFS chain of command.");

            DBSession.Delete(item);
            CommitChanges();

            return NoContent();
        }
    }
}