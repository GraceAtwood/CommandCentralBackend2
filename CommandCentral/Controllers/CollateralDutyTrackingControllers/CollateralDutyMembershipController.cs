using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Entities.CollateralDutyTracking;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.CollateralDutyTrackingControllers
{
    public class CollateralDutyMembershipController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.CollateralDutyMembership.Get>))]
        public IActionResult GetMembership([FromQuery] string level, [FromQuery] string role, [FromQuery] string person,
            [FromQuery] bool? hasDesignationLetter, [FromQuery] string collateralDuty)
        {
            var predicate = ((Expression<Func<CollateralDutyMembership, bool>>) null)
                .AddExactEnumQueryExpression(x => x.Level, level)
                .AddExactEnumQueryExpression(x => x.Role, role)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddNullableBoolQueryExpression(x => x.HasDesignationLetter, hasDesignationLetter);

            //Add a null safe AND phrase containing a disjunction for the name of the collateral duty or the id.
            if (!String.IsNullOrWhiteSpace(collateralDuty))
            {
                var subPredicate = collateralDuty.SplitByOr()
                    .Select(phrase =>
                    {
                        if (Guid.TryParse(phrase, out var id))
                            return x => x.Id == id;

                        return phrase.SplitByAnd()
                            .Aggregate((Expression<Func<CollateralDutyMembership, bool>>) null,
                                (current, term) => current.NullSafeAnd(x =>
                                    x.CollateralDuty.Name.Contains(term)));
                    })
                    .Aggregate<Expression<Func<CollateralDutyMembership, bool>>,
                        Expression<Func<CollateralDutyMembership, bool>>>(null,
                        (current, sub) => current.NullSafeOr(sub));

                predicate = predicate.NullSafeAnd(subPredicate);
            }

            var results = DBSession.Query<CollateralDutyMembership>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToList()
                .Select(x => new DTOs.CollateralDutyMembership.Get(x))
                .ToList();

            return Ok(results);
        }
    }
}