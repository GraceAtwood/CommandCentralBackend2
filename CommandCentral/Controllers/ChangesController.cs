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

namespace CommandCentral.Controllers
{
    /// <summary>
    /// Provides query access to the changes collection.  
    /// Changes themselves are controlled from within the API.  
    /// Clients do not have the ability to create new changes or modify changes directly.
    /// </summary>
    public class ChangesController : CommandCentralController
    {
        /// <summary>
        /// Queries the changes resource.  If the client does not have authorization to view the changed property for a 
        /// given person, then the property name and new and old value will be replaced with "REDACTED".
        /// </summary>
        /// <param name="editor">A person query for the person who made the edit.</param>
        /// <param name="entity">A person query for the person who was edited.</param>
        /// <param name="propertyPath">A string query for the path of the property that was modified.</param>
        /// <param name="oldValue">A string query for what a property's value used to be prior to an edit.</param>
        /// <param name="newValue">A string query for what a property's value changed to after the edit.</param>
        /// <param name="changeTime">A time range query for when the change took place.</param>
        /// <param name="limit">[Default=1000] Instructs the service to return no more than this many results.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Change.Get>))]
        public IActionResult Get([FromQuery] string editor, [FromQuery] string entity, [FromQuery] string propertyPath,
            [FromQuery] string oldValue, [FromQuery] string newValue, [FromQuery] DateTimeRangeQuery changeTime,
            [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var predicate = ((Expression<Func<Change, bool>>) null)
                .AddDateTimeQueryExpression(x => x.ChangeTime, changeTime)
                .AddStringQueryExpression(x => x.PropertyPath, propertyPath)
                .AddStringQueryExpression(x => x.OldValue, oldValue)
                .AddStringQueryExpression(x => x.NewValue, newValue)
                .AddPersonQueryExpression(x => x.Editor, editor)
                .AddEntityIdQueryExpression(x => x.Entity, entity);

            var changes = DBSession.Query<Change>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .OrderByDescending(x => x.ChangeTime)
                .Take(limit);

            foreach (var change in changes)
            {
                if (User.CanReturn(change.Entity, change.PropertyPath))
                    continue;

                change.PropertyPath = "REDACTED";
                change.OldValue = "REDACTED";
                change.NewValue = "REDACTED";
            }

            return Ok(changes.Select(x => new DTOs.Change.Get(x)));
        }
    }
}