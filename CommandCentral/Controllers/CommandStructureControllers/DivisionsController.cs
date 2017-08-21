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
using System.Linq.Expressions;
using NHibernate.Linq;
using LinqKit;

namespace CommandCentral.Controllers
{
    /// <summary>
    /// Provides query and get capability for divisions.
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class DivisionsController : CommandCentralController
    {
        /// <summary>
        /// Gets a list of divisions.
        /// </summary>
        /// <param name="name">A string query for the name property of a division.</param>
        /// <param name="description">A string query for the description property of a division.</param>
        /// <param name="department">A department query for the department of a division.</param>
        /// <returns></returns>
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Division.Get>))]
        public IActionResult Get([FromQuery] string name, [FromQuery] string description, [FromQuery] string department)
        {
            Expression<Func<Division, bool>> predicate = null;

            predicate = predicate
                .AddStringQueryExpression(x => x.Name, name)
                .AddStringQueryExpression(x => x.Description, description)
                .AddDepartmentQueryExpression(x => x.Department, department);

            var result = DBSession.Query<Division>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToFuture()
                .Select(item => new DTOs.Division.Get(item))
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a division by Id.
        /// </summary>
        /// <param name="id">The id of the division to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.Department.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<Division>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.Division.Get(item));
        }
    }
}
