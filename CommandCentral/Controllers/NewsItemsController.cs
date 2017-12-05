using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// <param name="creator"></param>
        /// <param name="title"></param>
        /// <param name="body"></param>
        /// <param name="creationTime"></param>
        /// <param name="limit"></param>
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
                .Select(x => new DTOs.NewsItem.Get(x))
                .ToList();

            return Ok(results);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.NewsItem.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<NewsItem>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.NewsItem.Get(item));
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.NewsItem.Get))]
        public IActionResult Post([FromBody] DTOs.NewsItem.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.SpecialPermissions.Contains(SpecialPermissions.EditNews))
                return Forbid();

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

            DBSession.Save(item);

            CommitChanges();

            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.NewsItem.Get(item));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.NewsItem.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.NewsItem.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.SpecialPermissions.Contains(SpecialPermissions.EditNews))
                return Forbid();

            var item = DBSession.Get<NewsItem>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            item.Body = dto.Body;
            item.Title = dto.Title;

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            CommitChanges();

            return CreatedAtAction(nameof(Put), new {id = item.Id}, new DTOs.NewsItem.Get(item));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (!User.SpecialPermissions.Contains(SpecialPermissions.EditNews))
                return Forbid();

            var item = DBSession.Get<NewsItem>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            DBSession.Delete(item);

            CommitChanges();

            return NoContent();
        }
    }
}