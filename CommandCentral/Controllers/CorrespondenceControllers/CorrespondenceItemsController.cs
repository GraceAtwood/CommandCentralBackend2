using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandCentral.Entities;
using CommandCentral.Entities.Correspondence;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Events.Args;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.CorrespondenceControllers
{
    /// <summary>
    /// Correspondence items encapsulate all types of correspondence such as SRCs and other administrative documents.  
    /// Items may be routed with physical, accompanying documentation.  This can be indicated in the .HasPhysicalCounterpart property.  
    /// A client should not submit reviews for a given item or modify it in any way unless they have seen that physical documentation.  
    /// This is only a business rule and not enforced in code in any way.  
    /// Correspondence reviews (/correspondenceitems/reviews) are a linked list containing all the reviews for a given item.  
    /// The last review is the one awaiting review.  The last review will have .NextReview = null, .ReviewedBy = null, .IsReviewed = false, and .IsRecommended = null.  
    /// The best way to find an item awaiting review is simply to use the hasBeenCompleted search parameter or the pendingReviewer parameter if you know who you're looking for.  
    /// </summary>
    public partial class CorrespondenceItemsController : CommandCentralController
    {
        /// <summary>
        /// Queries the correspondence items by the given criteria.
        /// If the client has access to the admin tools sub module, they will see all items; otherwise, 
        /// the client will only see items for which they are referenced anywhere in the item (reviewer, shared with, final approver, etc.) or are in the .SubmittedFor's chain of command.
        /// </summary>
        /// <param name="seriesNumbers">A query containing a list of series numbers to search for.</param>
        /// <param name="submittedFor">A query for the person who an item was submitted for.</param>
        /// <param name="submittedBy">A query for the person who submitted an item.</param>
        /// <param name="timeSubmitted">A time range query for the time an item was submitted.</param>
        /// <param name="hasAttachments">A boolean query for whether or not any attachments are on an item.</param>
        /// <param name="commentedBy">A person query for any items that have at least one comment that contains the given person.</param>
        /// <param name="hasComments">A boolean query for whether or not an item has comments.</param>
        /// <param name="hasReviews">A boolean query for whether or not a given item has any reviews.  A query of "false" here will show you all items that have not been routed to their first person yet.</param>
        /// <param name="reviewer">A person query for any item that has at least one review whose reviewer matches this item.</param>
        /// <param name="reviewedBy">A person query for any item that has at least one review whose reviewed by person matches this item.</param>
        /// <param name="sharedWith">A person query for any item that has been shared with the given person.</param>
        /// <param name="finalApprover">A person query for any item whose final approver is the given item.</param>
        /// <param name="pendingReviewer">A person query for any item that is waiting on the given person.  (Searches for an item with a review that has IsReviewed = false and Reviewer = given person).</param>
        /// <param name="hasBeenCompleted">A boolean query for any items that have not yet been completed.</param>
        /// <param name="hasPhysicalCounterpart">A boolean query for any items that were routed with a physical counterpart such as a folder of some kind.</param>
        /// <param name="body">A string query in the body of an item.</param>
        /// <param name="type">A reference list query for the type of correspondence item.</param>
        /// <param name="priorityLevel">An enum/string query for the priority of an item.</param>
        /// <param name="limit">[Default = 1000] Instructs the service to return no more than the given number of results.</param>
        /// <param name="orderBy">[Default = TimeSubmitted] Instructs the service to order the results by the given property.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<DTOs.CorrespondenceItem.Get>))]
        public IActionResult Get([FromQuery] string seriesNumbers, [FromQuery] string submittedFor,
            [FromQuery] string submittedBy,
            [FromQuery] DTOs.DateTimeRangeQuery timeSubmitted, [FromQuery] bool? hasAttachments,
            [FromQuery] string commentedBy,
            [FromQuery] bool? hasComments, [FromQuery] bool? hasReviews, [FromQuery] string reviewer,
            [FromQuery] string reviewedBy,
            [FromQuery] string sharedWith, [FromQuery] string finalApprover, [FromQuery] string pendingReviewer,
            [FromQuery] bool? hasBeenCompleted,
            [FromQuery] bool? hasPhysicalCounterpart, [FromQuery] string body, [FromQuery] string type,
            [FromQuery] string priorityLevel,
            [FromQuery] int limit = 1000, [FromQuery] string orderBy = nameof(CorrespondenceItem.TimeSubmitted))
        {
            if (limit <= 0)
                return BadRequestLimit(limit, nameof(limit));

            //Here we're just going to define some subqueries before we do the final search.  This will clean up the query syntax a bit.
            var commentedBySearch =
                CommonQueryStrategies.GetPersonQueryExpression<Comment>(y => y.Creator, commentedBy);
            var reviewerSearch =
                CommonQueryStrategies.GetPersonQueryExpression<CorrespondenceReview>(y => y.Reviewer, reviewer);
            var reviewedBySearch =
                CommonQueryStrategies.GetPersonQueryExpression<CorrespondenceReview>(y => y.ReviewedBy, reviewedBy);
            var sharedWithSearch = CommonQueryStrategies.GetPersonQueryExpression<Person>(y => y, reviewedBy);

            var predicate = ((Expression<Func<CorrespondenceItem, bool>>) null)
                .AddPersonQueryExpression(x => x.FinalApprover, finalApprover)
                .AddPersonQueryExpression(x => x.SubmittedBy, submittedBy)
                .AddPersonQueryExpression(x => x.SubmittedFor, submittedFor)
                .AddNullableBoolQueryExpression(x => x.HasBeenCompleted, hasBeenCompleted)
                .AddNullableBoolQueryExpression(x => x.HasPhysicalCounterpart, hasPhysicalCounterpart)
                .AddIntQueryExpression(x => x.SeriesNumber, seriesNumbers)
                .AddReferenceListQueryExpression(x => x.Type, type)
                .AddStringQueryExpression(x => x.Body, body)
                .AddDateTimeQueryExpression(x => x.TimeSubmitted, timeSubmitted);

            if (!String.IsNullOrWhiteSpace(commentedBy))
                predicate = predicate.NullSafeAnd(x => x.Comments.Any(commentedBySearch.Compile()));

            if (!String.IsNullOrWhiteSpace(reviewedBy))
                predicate = predicate.NullSafeAnd(x => x.Reviews.Any(reviewerSearch.Compile()));

            if (!String.IsNullOrWhiteSpace(reviewer))
                predicate = predicate.NullSafeAnd(x => x.Reviews.Any(reviewedBySearch.Compile()));

            if (!String.IsNullOrWhiteSpace(sharedWith))
                predicate = predicate.NullSafeAnd(x => x.SharedWith.Any(sharedWithSearch.Compile()));

            if (!String.IsNullOrWhiteSpace(pendingReviewer))
            {
                predicate = predicate.NullSafeAnd(y => y.Reviews.Any(
                    CommonQueryStrategies
                        .GetPersonQueryExpression<CorrespondenceReview>(x => x.Reviewer, pendingReviewer)
                        .NullSafeAnd(x => x.IsFinal == false)
                        .Compile()));
            }

            if (hasAttachments.HasValue)
                predicate = predicate.NullSafeAnd(x => x.Attachments.Any());

            if (hasComments.HasValue)
                predicate = predicate.NullSafeAnd(x => x.Comments.Any());

            if (hasReviews.HasValue)
                predicate = predicate.NullSafeAnd(x => x.Reviews.Any());

            var query = DBSession.Query<CorrespondenceItem>()
                .AsExpandable()
                .NullSafeWhere(predicate);

            if (String.Equals(orderBy, nameof(CorrespondenceItem.TimeSubmitted),
                StringComparison.CurrentCultureIgnoreCase))
                query = query.OrderByDescending(x => x.TimeSubmitted);
            else
                return BadRequest(
                    $"Your requested value '{orderBy}' for the parameter '{nameof(orderBy)}' is not supported.  The supported values are '{nameof(CorrespondenceItem.TimeSubmitted)}' ... and nothing else.  :/  If you need other order by properties here, just tell me.");

            var result = query
                .Take(limit)
                .ToList()
                .Where(x => x.CanPersonViewItem(User))
                .Select(item => new DTOs.CorrespondenceItem.Get(item));

            return Ok(result.ToList());
        }

        /// <summary>
        /// Retrieves the item identified by the given Id.  Client must have access to the admin tools submodules or be referenced in the item.
        /// </summary>
        /// <param name="id">The identifier for an item.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DTOs.CorrespondenceItem.Get))]
        public IActionResult Get(Guid id)
        {
            var item = DBSession.Get<CorrespondenceItem>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (!item.CanPersonViewItem(User))
                return Forbid();

            return Ok(new DTOs.CorrespondenceItem.Get(item));
        }

        /// <summary>
        /// Creates a new corr item.  Client must have access to the admin tools submodules or be referenced in the item.
        /// </summary>
        /// <param name="dto">A dto containing the information needed to create a new corr item.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(DTOs.CorrespondenceItem.Get))]
        public IActionResult Post([FromBody] DTOs.CorrespondenceItem.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var submittedFor = DBSession.Get<Person>(dto.SubmittedFor);
            if (submittedFor == null)
                return NotFoundParameter(dto.SubmittedFor, nameof(dto.SubmittedFor));

            var finalApprover = DBSession.Get<Person>(dto.FinalApprover);
            if (finalApprover == null)
                return NotFoundParameter(dto.FinalApprover, nameof(dto.FinalApprover));

            var type = DBSession.Get<CorrespondenceItemType>(dto.Type);
            if (type == null)
                return NotFoundParameter(dto.Type, nameof(dto.Type));

            var item = new CorrespondenceItem
            {
                Type = type,
                Body = dto.Body,
                FinalApprover = finalApprover,
                HasPhysicalCounterpart = dto.HasPhysicalCounterpart,
                Id = Guid.NewGuid(),
                SeriesNumber = (DBSession.Query<CorrespondenceItem>().Max(x => (int?) x.SeriesNumber) ?? 0) + 1,
                SubmittedBy = User,
                SubmittedFor = submittedFor,
                TimeSubmitted = CallTime
            };

            var result = item.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            if (!item.CanPersonEditItem(User))
                return Forbid();

            DBSession.Save(item);

            CommitChanges();
            Events.EventManager.OnCorrespondenceCreated(new CorrespondenceItemEventArgs {Item = item}, this);

            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.CorrespondenceItem.Get(item));
        }

        /// <summary>
        /// Modifies a correspondence item.
        /// </summary>
        /// <param name="id">The id of the corr item to modify.</param>
        /// <param name="dto">A dto containing the information needed to modify a corr item.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(201, Type = typeof(DTOs.CorrespondenceItem.Get))]
        public IActionResult Put(Guid id, [FromBody] DTOs.CorrespondenceItem.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var item = DBSession.Get<CorrespondenceItem>(id);
            if (item == null)
                return NotFoundParameter(id, nameof(id));

            if (item.HasBeenCompleted)
                return Conflict(
                    "The correspondence item has been completed.  Further modifications of it and its reviews is no longer allowed.");

            if (!item.CanPersonEditItem(User))
                return Forbid();

            var finalApprover = DBSession.Get<Person>(dto.FinalApprover);
            if (finalApprover == null)
                return NotFoundParameter(dto.FinalApprover, nameof(dto.FinalApprover));

            item.FinalApprover = finalApprover;
            item.Body = dto.Body;
            item.HasPhysicalCounterpart = dto.HasPhysicalCounterpart;

            CommitChanges();

            Events.EventManager.OnCorrespondenceModified(new Events.Args.CorrespondenceItemEventArgs
            {
                Item = item
            }, this);

            return CreatedAtAction(nameof(Get), new {id = item.Id}, new DTOs.CorrespondenceItem.Get(item));
        }

        /// <summary>
        /// Deletes a corr item.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult Delete(Guid id)
        {
            var item = DBSession.Get<CorrespondenceItem>(id);
            if (item == null)
                return NotFound();

            if (item.HasBeenCompleted)
                return Conflict(
                    "The correspondence item has been completed.  Further modifications of it and its reviews is no longer allowed.");

            if (!item.CanPersonEditItem(User))
                return Forbid();

            DBSession.Delete(item);

            CommitChanges();

            Events.EventManager.OnCorrespondenceDeleted(new Events.Args.CorrespondenceItemEventArgs
            {
                Item = item
            }, this);

            return NoContent();
        }
    }
}