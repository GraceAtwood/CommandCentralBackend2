using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Authorization;
using CommandCentral.Enums;

namespace CommandCentral.Controllers
{
    public class NewsItemsController : CommandCentralController
    {
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.NewsItem.Get>))]
        public IActionResult Get()
        {
            var items = DBSession.Query<NewsItem>().ToList();

            return Ok(items.Select(x => new DTOs.NewsItem.Get(x)));
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

            DBSession.Save(item);
            
            CommitChanges();

            return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.NewsItem.Get(item));
        }

        [HttpPut("{id}")]
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

            CommitChanges();

            return CreatedAtAction(nameof(Put), new { id = item.Id }, new DTOs.NewsItem.Get(item));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.EditNews))
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
