using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Entities;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.CommandStructureControllers
{
    /// <summary>
    /// Provides access to departments and a department's collection of divisions.
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public partial class DepartmentsController : CommandCentralController
    {
        /// <summary>
        /// Gets a list of department.
        /// </summary>
        /// <param name="name">A string query for the name property of a department.</param>
        /// <param name="description">A string query for the description property of a department.</param>
        /// <param name="command">A command query for the command of a department.</param>
        /// <returns></returns>
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Department.Get>))]
        public IActionResult Get([FromQuery] string name, [FromQuery] string description, [FromQuery] string command)
        {
            var predicate = ((Expression<Func<Department, bool>>) null)
                .AddStringQueryExpression(x => x.Name, name)
                .AddStringQueryExpression(x => x.Description, description)
                .AddCommandQueryExpression(x => x.Command, command);

            var result = DBSession.Query<Department>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToFuture()
                .Select(item => new DTOs.Department.Get(item))
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a department by Id.
        /// </summary>
        /// <param name="id">The id of the department to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.Department.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<Department>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.Department.Get(item));
        }
    }
}
