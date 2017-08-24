using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.AccountManagementControllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class PasswordResetController : CommandCentralController
    {
        /// <summary>
        /// Allows only clients with admin tools to query the password resets.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="timeSubmitted"></param>
        /// <returns></returns>
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.PasswordReset.Get>))]
        public IActionResult Get([FromQuery] string person, [FromQuery] DTOs.DateTimeRangeQuery timeSubmitted)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var predicate = ((Expression<Func<PasswordReset, bool>>) null)
                .AddPersonQueryExpression(x => x.Person, person)
                .AddDateTimeQueryExpression(x => x.TimeSubmitted, timeSubmitted);

            var results = DBSession.Query<PasswordReset>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToList()
                .Select(x => new DTOs.PasswordReset.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Allows only clients with admin tools to get password resets by id.
        /// </summary>
        /// <param name="id">The reset object's id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.PasswordReset.Get))]
        public IActionResult Get(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            var confirmation = DBSession.Get<PasswordReset>(id);
            if (confirmation == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.PasswordReset.Get(confirmation));
        }
    }
}
