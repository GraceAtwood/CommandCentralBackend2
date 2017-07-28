using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.DTOs;
using CommandCentral.Authorization;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class NewsItemController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        public IActionResult Get()
        {
            var items = DBSession.QueryOver<NewsItem>().List();
            return Ok(items.Select(x => new NewsItemDTO
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
            return Ok(new NewsItemDTO
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
        public IActionResult Post([FromBody]NewsItemDTO dto)
        {
            if (!new ResolvedPermissions(User, null).AccessibleSubmodules.Contains(Enums.SubModules.EditNews))
                return PermissionDenied();

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

                var result = new NewsItem.NewsItemValidator().Validate(item);
                if (!result.IsValid)
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));

                DBSession.Save(item);
                transaction.Commit();

                return CreatedAtAction(nameof(Get), new { id = item.Id }, new NewsItemDTO
                {
                    Body = item.Body,
                    CreationTime = item.CreationTime,
                    Creator = item.Creator.Id,
                    Id = item.Id,
                    Title = item.Title
                });
            }
            
        }

        [HttpPatch("{id}")]
        [RequireAuthentication]
        public IActionResult Patch(Guid id, [FromBody]NewsItemDTO dto)
        {
            if (!new ResolvedPermissions(User, null).AccessibleSubmodules.Contains(Enums.SubModules.EditNews))
                return PermissionDenied();

            using (var transaction = DBSession.BeginTransaction())
            {
                var item = DBSession.Get<NewsItem>(id);
                if (item == null)
                {
                    return BadRequest("No NewsItem with that Id exists.");
                }

                item.Body = dto.Body;
                item.Title = dto.Title;

                var result = new NewsItem.NewsItemValidator().Validate(item);
                if (!result.IsValid)
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));

                DBSession.Update(item);
                transaction.Commit();

                return NoContent();
            }
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        public IActionResult Delete(Guid id)
        {
            if (!new ResolvedPermissions(User, null).AccessibleSubmodules.Contains(Enums.SubModules.EditNews))
                return PermissionDenied();

            using (var transaction = DBSession.BeginTransaction())
            {
                var item = DBSession.Get<NewsItem>(id);
                if (item == null)
                {
                    return BadRequest("I mean, technically it's deleted? Since, like, no NewsItem with that Id existed at all...");
                }

                DBSession.Delete(item);
                transaction.Commit();

                return NoContent();
            }
        }
    }
}
