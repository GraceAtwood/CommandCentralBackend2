using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Framework;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;

namespace CommandCentral.Controllers.PersonProfileControllers
{
    public partial class PersonsController
    {
        /// <summary>
        /// Retrieves all changes for the given person ordered by the change time.
        /// </summary>
        /// <param name="personId">The person for whom to retrieve changes.</param>
        /// <param name="limit">Instructst the service to retrieve no more than this number of results.</param>
        /// <returns></returns>
        [HttpGet("{personId}/Changes")]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Change.Get>))]
        public IActionResult GetChanges(Guid personId, [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var items = DBSession.Query<Change>()
                .Where(x => x.Person.Id == personId)
                .OrderByDescending(x => x.ChangeTime)
                .Take(limit)
                .ToList();

            if (!items.Any())
                return Ok(new List<DTOs.AccountHistoryEvent.Get>());

            if (!User.GetFieldPermissions<Person>(items.First().Person).CanReturn(x => x.Changes))
                return Forbid();

            return Ok(items.Select(item => new DTOs.Change.Get(item)).ToList());
        }
    }
}

