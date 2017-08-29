using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Authorization;
using CommandCentral.Enums;
using NHibernate.Linq;

namespace CommandCentral.Controllers
{
    public class NewsItemsController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.NewsItem.Get>))]
        public IActionResult Get()
        {
            var items = DBSession.Query<NewsItem>().ToList();

            return Ok(items.Select(x => new DTOs.NewsItem.Get(x)));
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.NewsItem.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<NewsItem>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            return Ok(new DTOs.NewsItem.Get(item));
        }

        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.NewsItem.Get))]
        public IActionResult Post([FromBody]DTOs.NewsItem.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            if (!User.CanAccessSubmodules(SubModules.EditNews))
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

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.NewsItem.Get(item));
        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.NewsItem.Get))]
        public IActionResult Put(Guid id, [FromBody]DTOs.NewsItem.Update dto)
        {
            if (dto == null)
                return BadRequestDTONull();
            
            if (!User.CanAccessSubmodules(SubModules.EditNews))
                return Forbid();

            var item = DBSession.Get<NewsItem>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            item.Body = dto.Body;
            item.Title = dto.Title;

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Update(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(Put), new { id = item.Id }, new DTOs.NewsItem.Get(item));
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.EditNews))
                return Forbid();

            var item = DBSession.Get<NewsItem>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Delete(item);
                transaction.Commit();
            }

            return NoContent();
        }
    }
}
