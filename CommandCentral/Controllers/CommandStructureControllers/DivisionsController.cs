using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Entities;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.CommandStructureControllers
{
    /// <summary>
    /// Provides query and get capability for divisions.
    /// </summary>
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
        [ProducesResponseType(200, Type = typeof(List<DTOs.Division.Get>))]
        public IActionResult Get([FromQuery] string name, [FromQuery] string description, [FromQuery] string department)
        {
            var predicate = ((Expression<Func<Division, bool>>) null)
                .AddStringQueryExpression(x => x.Name, name)
                .AddStringQueryExpression(x => x.Description, description)
                .AddDepartmentQueryExpression(x => x.Department, department);

            var result = DBSession.Query<Division>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .ToList()
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
