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
    public class CommentsController : CommandCentralController
    {
        [HttpGet("{id}")]
        [RequireAuthentication]
        public IActionResult Get(Guid id)
        {
            var result = DBSession.QueryOver<Comment>().Where(Restrictions.Eq("OwningEntity.id", id)).List();

            return Ok(result.Select(x =>
                new CommentDTO
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
        public IActionResult Post([FromBody]CommentDTO dto)
        {
            using (var transaction = DBSession.BeginTransaction())
            {
                try
                {
                    if (DBSession.QueryOver<ICommentable>().Where(x => x.Id == dto.OwningEntity).ToRowCountQuery().List<int>().Sum() == 0)
                        return BadRequest("Your owning entity does not exist.");

                    var owningEntity = DBSession.QueryOver<ICommentable>()
                        .Where(x => x.Id == dto.OwningEntity)
                        .SingleOrDefault();

                    if (!owningEntity.CanPersonAccessComments(User))
                        return Unauthorized();

                    var comment = new Comment
                    {
                        Body = dto.Body,
                        Creator = User,
                        Id = Guid.NewGuid(),
                        OwningEntity = owningEntity,
                        TimeCreated = CallTime
                    };

                    var result = new Comment.CommentValidator().Validate(comment);
                    if (!result.IsValid)
                        return BadRequest(result.Errors.Select(x => x.ErrorMessage));

                    DBSession.Save(comment);
                    transaction.Commit();
                    return Ok(comment.Id);
                }
                catch (Exception e)
                {
                    LogException(e);
                    transaction.Rollback();
                    return InternalServerError();
                }
            }

        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
