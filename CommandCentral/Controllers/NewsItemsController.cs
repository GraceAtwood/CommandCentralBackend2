using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.DTOs;
using CommandCentral.Authorization;
using CommandCentral.Enums;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class NewsItemsController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        public IActionResult Get()
        {
            var items = DBSession.QueryOver<NewsItem>().List();

            return Ok(items.Select(x => new DTOs.NewsItem.Get
            {
                Id = x.Id,
                Body = x.Body,
                Title = x.Title,
                CreationTime = x.CreationTime,
                Creator = x.Creator.Id
            }));
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<NewsItem>(id);
            if (item == null)
                return NotFound();

            return Ok(new DTOs.NewsItem.Get
            {
                Id = item.Id,
                Body = item.Body,
                Title = item.Title,
                CreationTime = item.CreationTime,
                Creator = item.Creator.Id
            });
        }

        [HttpPost]
        [RequireAuthentication]
        public IActionResult Post([FromBody]DTOs.NewsItem.Update dto)
        {
            if (!User.CanAccessSubmodules(SubModules.EditNews))
                return Forbid();

            using (var transaction = DBSession.BeginTransaction())
            {
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
                transaction.Commit();

                return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.NewsItem.Get
                {
                    Body = item.Body,
                    CreationTime = item.CreationTime,
                    Creator = item.Creator.Id,
                    Id = item.Id,
                    Title = item.Title
                });
            }
            
        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        public IActionResult Put(Guid id, [FromBody]DTOs.NewsItem.Update dto)
        {
            if (!User.CanAccessSubmodules(SubModules.EditNews))
                return Forbid();

            using (var transaction = DBSession.BeginTransaction())
            {
                var item = DBSession.Get<NewsItem>(id);
                if (item == null)
                    return NotFound();

                item.Body = dto.Body;
                item.Title = dto.Title;

                var result = item.Validate();
                if (!result.IsValid)
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));

                DBSession.Update(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Put), new { id = item.Id }, new DTOs.NewsItem.Get
                {
                    Body = item.Body,
                    CreationTime = item.CreationTime,
                    Creator = item.Creator.Id,
                    Id = item.Id,
                    Title = item.Title
                });
            }
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        public IActionResult Delete(Guid id)
        {
            if (!User.CanAccessSubmodules(SubModules.EditNews))
                return Forbid();

            using (var transaction = DBSession.BeginTransaction())
            {
                var item = DBSession.Get<NewsItem>(id);
                if (item == null)
                    return NotFound();

                DBSession.Delete(item);
                transaction.Commit();

                return Ok();
            }
        }
    }
}
