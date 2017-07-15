using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.DTOs;
using CommandCentral.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class NewsItemController : CommandCentralController
    {
        // GET: api/values
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

        // GET api/values/5
        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<NewsItem>(id);
            return Ok(new NewsItemDTO
            {
                Id = item.Id,
                Body = item.Body,
                Title = item.Title,
                CreationTime = item.CreationTime,
                Creator = item.Creator.Id
            });
        }

        // POST api/values
        [HttpPost]
        [RequireAuthentication]
        public IActionResult Post([FromBody]NewsItemDTO dto)
        {
            var canEditNews = new ResolvedPermissions(User, null).AccessibleSubmodules.Contains(Enums.SubModules.EditNews);
            if (!canEditNews) return Unauthorized();
            using (var transaction = DBSession.BeginTransaction())
            {
                try
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
                    return Ok(item.Id);
                }
                catch (Exception e)
                {
                    LogException(e);
                    transaction.Rollback();
                    return InternalServerError();
                }
            }
            
        }

        // PATCH api/values/5
        [HttpPatch("{id}")]
        [RequireAuthentication]
        public IActionResult Patch(Guid id, [FromBody]NewsItemDTO dto)
        {
            var canEditNews = new ResolvedPermissions(User, null).AccessibleSubmodules.Contains(Enums.SubModules.EditNews);
            if (!canEditNews) return Unauthorized();
            using (var transaction = DBSession.BeginTransaction())
            {
                try
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
                    return Ok(id);

                }
                catch (Exception e)
                {
                    LogException(e);
                    transaction.Rollback();
                    return InternalServerError();
                }
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        [RequireAuthentication]
        public IActionResult Delete(Guid id)
        {
            var canEditNews = new ResolvedPermissions(User, null).AccessibleSubmodules.Contains(Enums.SubModules.EditNews);
            if (!canEditNews) return Unauthorized();
            using (var transaction = DBSession.BeginTransaction())
            {
                try
                {
                    var item = DBSession.Get<NewsItem>(id);
                    if (item == null)
                    {
                        return BadRequest("I mean, technically it's deleted? Since, like, no NewsItem with that Id existed at all...");
                    }
                    DBSession.Delete(item);
                    transaction.Commit();
                    return Ok();
                }
                catch (Exception e)
                {
                    LogException(e);
                    transaction.Rollback();
                    return InternalServerError();
                }
            }
        }
    }
}
