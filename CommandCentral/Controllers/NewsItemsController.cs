using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Formatting;
using CommandCentral.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Framework.Data;
using LinqKit;

namespace CommandCentral.Controllers
{
    /// <summary>
    /// Provides access to the news items collection.
    /// </summary>
    public class NewsItemsController : CommandCentralController
    {
        /// <summary>
        /// Queries the news items collection.
        /// </summary>
        /// <param name="creator">A person query for the creator of a news item.</param>
        /// <param name="title">A string query for the title.</param>
        /// <param name="body">A string query for the body.</param>
        /// <param name="creationTime">A date time range query for the creation time.</param>
        /// <param name="limit">[Optional][Default = 1000] Instructs the service to return no more than this number of results.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<DTOs.NewsItem.Get>), 200)]
        public IActionResult Get([FromQuery] string creator, [FromQuery] string title, [FromQuery] string body,
            [FromQuery] DTOs.DateTimeRangeQuery creationTime, [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var predicate = ((Expression<Func<NewsItem, bool>>) null)
                .AddStringQueryExpression(x => x.Body, body)
                .AddStringQueryExpression(x => x.Title, title)
                .AddPersonQueryExpression(x => x.Creator, creator)
                .AddDateTimeQueryExpression(x => x.CreationTime, creationTime);

            var results = DBSession.Query<NewsItem>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .OrderByDescending(x => x.CreationTime)
                .Take(limit)
                .ToList()
                .Where(x => User.CanReturn(x))
                .Select(x => new DTOs.NewsItem.Get(x))
                .ToList();

            return Ok(results);
        }

        /// <summary>
        /// Retrieves a news item.
        /// </summary>
        /// <param name="id">The id of the news item to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.NewsItem.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<NewsItem>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanReturn(item))
                return Forbid("You can't view this item.");

            return Ok(new DTOs.NewsItem.Get(item));
        }

        /// <summary>
        /// Creates a news item.
        /// </summary>
        /// <param name="dto">A dto containing the information needed to create a news item.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.NewsItem.Get))]
        public IActionResult Post([FromBody] DTOs.NewsItem.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var item = new NewsItem
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Body = dto.Body,
                Creator = User,
                CreationTime = CallTime
            };

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            if (!User.CanEdit(item))
                return Forbid("You can't modify this news item.");

            DBSession.Save(item);
            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.NewsItem.Get(item));
        }

        /// <summary>
        /// Modifies a news item.
        /// </summary>
        /// <param name="id">The id of the news item to modify.</param>
        /// <param name="dto">A dto containing the information needed to modify a news item.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.NewsItem.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.NewsItem.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var item = DBSession.Get<NewsItem>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(item))
                return Forbid("You can't modify this news item.");

            item.Body = dto.Body;
            item.Title = dto.Title;

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            CommitChanges();

            return CreatedAtAction(nameof(Put), new {id = item.Id}, new DTOs.NewsItem.Get(item));
        }

        /// <summary>
        /// Deletes a news item.
        /// </summary>
        /// <param name="id">The id of the news item to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var item = DBSession.Get<NewsItem>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (!User.CanEdit(item))
                return Forbid("You can't modify this news item.");

            Delete(item);
            CommitChanges();

            return NoContent();
        }
    }
}