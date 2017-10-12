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
    }
}