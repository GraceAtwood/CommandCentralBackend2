using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.DTOs;
using System.Linq.Expressions;
using CommandCentral.Utilities;
using CommandCentral.Framework.Data;
using LinqKit;
using CommandCentral.Authorization;
using CommandCentral.Enums;

namespace CommandCentral.Controllers
{
    /// <summary>
    /// Comments are implemented by multiple entities and allow clients to post... well... comments to an entity.
    /// Permissions for who can access and post comments to an entity are controlled by that entity.
    /// </summary>
    public class CommentController : CommandCentralController
    {
        /// <summary>
        /// Queries all comments for the given criteria.
        /// NOTE: Results are filtered after database load based on whether or not your client can view the given comments.  
        /// For this reason, limit should be seen as "return no more than this number of results".
        /// </summary>
        /// <param name="owningEntity">Id of the entity that owns the comments.</param>
        /// <param name="creator">The person who created a comment.</param>
        /// <param name="timeCreated">A time range describing the dates to search for when a comment was created.</param>
        /// <param name="body">Queries the body of comments.</param>
        /// <param name="limit">[Default = 1000] Instructs the service to return no more than this number of results.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.Command.Get>))]
        public IActionResult Get([FromQuery] Guid? owningEntity, [FromQuery] string creator, [FromQuery] DateTimeRangeQuery timeCreated,
            [FromQuery] string body, [FromQuery] int limit = 1000)
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            var predicate = ((Expression<Func<Comment, bool>>) null)
                .AddStringQueryExpression(x => x.Body, body)
                .AddPersonQueryExpression(x => x.Creator, creator)
                .AddDateTimeQueryExpression(x => x.TimeCreated, timeCreated);

            if (owningEntity.HasValue)
                predicate = predicate.NullSafeAnd(x => x.OwningEntity.Id == owningEntity);

            var result = DBSession.Query<Comment>()
                .AsExpandable()
                .NullSafeWhere(predicate)
                .OrderByDescending(x => x.TimeCreated)
                .Take(limit)
                .ToList()
                .Where(item => item.OwningEntity.CanPersonAccessComments(User))
                .Select(item => new DTOs.Comment.Get(item))
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Retrieves the comment identified by the given id.
        /// </summary>
        /// <param name="id">The identifier for a given comment.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.Command.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<Comment>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (!item.OwningEntity.CanPersonAccessComments(User))
                return Forbid();

            return Ok(new DTOs.Comment.Get(item));
        }

        /// <summary>
        /// Creates a new comment.  The creator of the new comment will be the currently logged in user.
        /// </summary>
        /// <param name="dto">A dto containing all the information required to make a new comment.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.Command.Get))]
        public IActionResult Post([FromBody]DTOs.Comment.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var owningEntity = DBSession.Query<IHazComments>()
                .SingleOrDefault(x => x.Id == dto.OwningEntity);

            if (owningEntity == null)
                return NotFoundParameter(dto.OwningEntity, nameof(dto.OwningEntity));

            if (!owningEntity.CanPersonAccessComments(User))
                return Forbid();

            var item = new Comment
            {
                Body = dto.Body,
                Creator = User,
                Id = Guid.NewGuid(),
                OwningEntity = owningEntity,
                TimeCreated = CallTime
            };

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            DBSession.Save(item);
            
            CommitChanges();

            return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.Comment.Get(item));
        }

        /// <summary>
        /// Modifies the comment identified by the given id.  The client must own the comment or have access to admin tools to modify a comment.
        /// </summary>
        /// <param name="id">An identifier for the comment you want to modify.</param>
        /// <param name="dto">A dto containing all the information required to modify the given comment.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.Command.Get))]
        public IActionResult Put(Guid id, [FromBody]DTOs.Comment.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var item = DBSession.Get<Comment>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (item.Creator != User || !User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            item.Body = dto.Body;

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            CommitChanges();
            
            return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.Comment.Get(item));
        }

        /// <summary>
        /// Deletes the given comment.  The client must own the comment or have access to admin tools to modify a comment.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var item = DBSession.Get<Comment>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (item.Creator != User || !User.CanAccessSubmodules(SubModules.AdminTools))
                return Forbid();

            DBSession.Delete(item);
            
            CommitChanges();

            return NoContent();
        }
    }
}
