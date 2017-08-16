using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommandCentral.Framework;
using CommandCentral.Entities;
using CommandCentral.Utilities;
using CommandCentral.Framework.Data;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Authorization;
using CommandCentral.Enums;
using NHibernate.Linq;
using CommandCentral.Entities.Correspondence;
using System.Linq.Expressions;
using LinqKit;

namespace CommandCentral.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CorrespondenceItemsController : CommandCentralController
    {
        [HttpGet]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(List<DTOs.CorrespondenceItem.Get>))]
        public IActionResult Get([FromQuery] string seriesNumbers, [FromQuery] string submittedFor, [FromQuery] string submittedBy,
            [FromQuery] DTOs.DateTimeRangeQuery timeSubmitted, [FromQuery] bool? hasAttachments, [FromQuery] string commentedBy,
            [FromQuery] bool? hasComments, [FromQuery] bool? hasReviews, [FromQuery] string reviewer, [FromQuery] string reviewedBy,
            [FromQuery] string sharedWith, [FromQuery] string finalApprover, [FromQuery] string pendingReviewer, [FromQuery] bool? hasBeenCompleted,
            [FromQuery] bool? hasPhyisicalCounterpart, [FromQuery] string body, [FromQuery] string type,
            [FromQuery] int limit = 1000, [FromQuery] string orderBy = nameof(CorrespondenceItem.TimeSubmitted))
        {
            if (limit <= 0)
                return BadRequest($"The value '{limit}' for the property '{nameof(limit)}' was invalid.  It must be greater than zero.");

            Expression<Func<Comment, bool>> commentedBySearch = CommonQueryStrategies.GetPersonQueryExpression<Comment>(y => y.Creator, commentedBy);
            Expression<Func<CorrespondenceReview, bool>> reviewerSearch = CommonQueryStrategies.GetPersonQueryExpression<CorrespondenceReview>(y => y.Reviewer, reviewer);
            Expression<Func<CorrespondenceReview, bool>> reviewedBySearch = CommonQueryStrategies.GetPersonQueryExpression<CorrespondenceReview>(y => y.ReviewedBy, reviewedBy);
            Expression<Func<Person, bool>> sharedWithSearch = CommonQueryStrategies.GetPersonQueryExpression<Person>(y => y, reviewedBy);

            Expression<Func<CorrespondenceItem, bool>> predicate = null;

            predicate = predicate
                .AddPersonQueryExpression(x => x.FinalApprover, finalApprover)
                .AddPersonQueryExpression(x => x.SubmittedBy, submittedBy)
                .AddPersonQueryExpression(x => x.SubmittedFor, submittedFor)
                .AddNullableBoolQueryExpression(x => x.HasBeenCompleted, hasBeenCompleted)
                .AddNullableBoolQueryExpression(x => x.HasPhysicalCounterpart, hasPhyisicalCounterpart)
                .AddIntQueryExpression(x => x.SeriesNumber, seriesNumbers)
                .NullSafeAnd(x => x.Comments.Any(commentedBySearch.Compile()))
                .NullSafeAnd(x => x.Reviews.Any(reviewerSearch.Compile()))
                .NullSafeAnd(x => x.Reviews.Any(reviewedBySearch.Compile()))
                .NullSafeAnd(x => x.SharedWith.Any(sharedWithSearch.Compile()))
                .AddReferenceListQueryExpression(x => x.Type, type)
                .AddStringQueryExpression(x => x.Body, body)
                .AddDateTimeQueryExpression(x => x.TimeSubmitted, timeSubmitted);

            if (!String.IsNullOrWhiteSpace(pendingReviewer))
            {
                predicate = predicate.NullSafeAnd(y => y.Reviews.Any(
                    CommonQueryStrategies.GetPersonQueryExpression<CorrespondenceReview>(x => x.Reviewer, pendingReviewer)
                    .NullSafeAnd(x => x.IsFinal == false)
                    .Compile()));
            }

            if (hasAttachments.HasValue)
                predicate = predicate.NullSafeAnd(x => x.Attachments.Count() > 0);

            if (hasComments.HasValue)
                predicate = predicate.NullSafeAnd(x => x.Comments.Count() > 0);

            if (hasReviews.HasValue)
                predicate = predicate.NullSafeAnd(x => x.Reviews.Count() > 0);
            
            //This query will add the permissions restrictions
            if (!User.CanAccessSubmodules(SubModules.AdminTools))
                predicate = predicate.NullSafeAnd(x => x.SubmittedBy == User || x.SubmittedFor == User || 
                    x.Reviews.Any(y => y.Reviewer == User || y.ReviewedBy == User) || x.SharedWith.Contains(User));

            var query = DBSession.Query<CorrespondenceItem>()
                .AsExpandable()
                .Where(predicate);

            if (String.Equals(orderBy, nameof(CorrespondenceItem.TimeSubmitted), StringComparison.CurrentCultureIgnoreCase))
                query = query.OrderByDescending(x => x.TimeSubmitted);
            else
                return BadRequest($"Your requested value '{orderBy}' for the parameter '{nameof(orderBy)}' is not supported.  The supported values are '{nameof(CorrespondenceItem.TimeSubmitted)}' ... and nothing else.  :/  If you need other order by properties here, just tell me.");

            var result = query
                .Take(limit)
                .ToList()
                .Select(item => new DTOs.CorrespondenceItem.Get(item));

            return Ok(result.ToList());
        }

        [HttpGet("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200, Type = typeof(DTOs.CorrespondenceItem.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<CorrespondenceItem>(id);
            if (item == null)
                return NotFound();

            if (item.SubmittedBy != User && item.SubmittedFor != User &&
                !item.Reviews.Any(x => x.Reviewer == User || x.ReviewedBy == User) &&
                !item.SharedWith.Contains(User))
                return Forbid();

            return Ok(new DTOs.CorrespondenceItem.Get(item));
        }

        [HttpPost]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.CorrespondenceItem.Get))]
        public IActionResult Post([FromBody]DTOs.CorrespondenceItem.Post dto)
        {
            if (dto == null)
                return BadRequest();

            var submittedFor = DBSession.Get<Person>(dto.SubmittedFor);
            if (submittedFor == null)
                return NotFound($"The object identified by your parameter '{nameof(dto.SubmittedFor)}' does not exist.");

            if (!User.CanAccessSubmodules(SubModules.AdminTools) && !User.IsInChainOfCommand(submittedFor))
                return Forbid();

            var finalApprover = DBSession.Get<Person>(dto.FinalApprover);
            if (finalApprover == null)
                return NotFound($"The object identified by your parameter '{nameof(dto.FinalApprover)}' does not exist.");

            var type = DBSession.Get<CorrespondenceItemType>(dto.Type);
            if (type == null)
                return NotFound($"The object identified by your parameter '{nameof(dto.Type)}' does not exist.");

            var item = new CorrespondenceItem
            {
                Type = type,
                Body = dto.Body,
                FinalApprover = finalApprover,
                HasPhysicalCounterpart = dto.HasPhysicalCounterpart,
                Id = Guid.NewGuid(),
                SeriesNumber = DBSession.Query<CorrespondenceItem>().Max(x => x.SeriesNumber) + 1,
                SubmittedBy = User,
                SubmittedFor = submittedFor,
                TimeSubmitted = CallTime
            };

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(item);
                transaction.Commit();
            }

            return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.CorrespondenceItem.Get(item));
        }

        [HttpPut("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(201, Type = typeof(DTOs.CorrespondenceItem.Get))]
        public IActionResult Put(Guid id, [FromBody]DTOs.CorrespondenceItem.Put dto)
        {
            if (dto == null)
                return BadRequest();

            var item = DBSession.Get<CorrespondenceItem>(id);
            if (item == null)
                return NotFound();

            var finalApprover = DBSession.Get<Person>(dto.FinalApprover);
            if (finalApprover == null)
                return NotFound($"The object identified by your parameter '{nameof(dto.FinalApprover)}' does not exist.");

            item.FinalApprover = finalApprover;
            item.Body = dto.Body;
            item.HasPhysicalCounterpart = dto.HasPhysicalCounterpart;

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Save(item);
                transaction.Commit();
            }

            Events.EventManager.OnCorrespondenceModified(new Events.Args.CorrespondenceItemEventArgs
            {
                Item = item
            }, this);

            return CreatedAtAction(nameof(Get), new { id = item.Id }, new DTOs.CorrespondenceItem.Get(item));
        }

        [HttpDelete("{id}")]
        [RequireAuthentication]
        [ProducesResponseType(200)]
        public IActionResult Delete(Guid id)
        {
            var item = DBSession.Get<CorrespondenceItem>(id);
            if (item == null)
                return NotFound();

            if (!User.CanAccessSubmodules(SubModules.AdminTools) && !User.IsInChainOfCommand(item.SubmittedFor))
                return Forbid();

            using (var transaction = DBSession.BeginTransaction())
            {
                DBSession.Delete(item);
                transaction.Commit();
            }

            Events.EventManager.OnCorrespondenceDeleted(new Events.Args.CorrespondenceItemEventArgs
            {
                Item = item
            }, this);

            return Ok();
        }
    }
}
