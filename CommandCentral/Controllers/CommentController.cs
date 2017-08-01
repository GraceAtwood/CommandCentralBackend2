using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.DTOs;
using CommandCentral.Utilities.Types;
using NHibernate.Criterion;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    public class CommentController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        public IActionResult Get([FromQuery]Guid owningEntity)
        {
            var result = DBSession.QueryOver<Comment>().Where(Restrictions.Eq("OwningEntity.id", owningEntity)).List();

            if (!result.Any())
                return NotFound();

            if (result.Any() && !result.First().OwningEntity.CanPersonAccessComments(User))
                return PermissionDenied();
            
            return Ok(result.Select(x =>
                new DTOs.Comment.Get
                {
                    Body = x.Body,
                    Creator = x.Creator.Id,
                    Id = x.Id,
                    OwningEntity = x.OwningEntity.Id,
                    TimeCreated = x.TimeCreated
                }
            ));
        }

        [HttpPost]
        [RequireAuthentication]
        public IActionResult Post([FromBody]DTOs.Comment.Post dto)
        {
            using (var transaction = DBSession.BeginTransaction())
            {
                if (DBSession.QueryOver<CommentableEntity>().Where(x => x.Id == dto.OwningEntity).ToRowCountQuery().List<int>().Sum() == 0)
                    return NotFound($"The parameter {nameof(dto.OwningEntity)} could not be found.");

                var owningEntity = DBSession.QueryOver<CommentableEntity>()
                    .Where(x => x.Id == dto.OwningEntity)
                    .SingleOrDefault();

                if (!owningEntity.CanPersonAccessComments(User))
                    return PermissionDenied();

                var comment = new Comment
                {
                    Body = dto.Body,
                    Creator = User,
                    Id = Guid.NewGuid(),
                    OwningEntity = owningEntity,
                    TimeCreated = CallTime
                };

                var result = comment.Validate();
                if (!result.IsValid)
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));

                DBSession.Save(comment);
                transaction.Commit();

                return CreatedAtAction(nameof(Get), new { id = comment.Id }, new DTOs.Comment.Get
                {
                    Body = comment.Body,
                    Creator = comment.Creator.Id,
                    Id = comment.Id,
                    OwningEntity = comment.OwningEntity.Id,
                    TimeCreated = comment.TimeCreated
                });
            }

        }

        [HttpPatch("{id}")]
        [RequireAuthentication]
        public IActionResult Patch(Guid id, [FromBody]DTOs.Comment.Patch dto)
        {
            using (var transaction = DBSession.BeginTransaction())
            {
                var comment = DBSession.Get<Comment>(id);

                if (comment == null)
                    return NotFound();

                if (!comment.OwningEntity.CanPersonAccessComments(User))
                    return PermissionDenied();

                if (comment.Creator.Id != User.Id)
                    return PermissionDenied();

                comment.Body = dto.Body;

                var result = comment.Validate();
                if (!result.IsValid)
                    return BadRequest(result.Errors.Select(x => x.ErrorMessage));

                DBSession.Update(comment);
                transaction.Commit();

                return NoContent();
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            using (var transaction = DBSession.BeginTransaction())
            {
                var comment = DBSession.Get<Comment>(id);

                if (comment == null)
                    return NotFound();

                if (!comment.OwningEntity.CanPersonAccessComments(User))
                    return PermissionDenied();

                if (comment.Creator.Id != User.Id)
                    return PermissionDenied();

                DBSession.Delete(comment);
                transaction.Commit();

                return NoContent();
            }
        }
    }
}
