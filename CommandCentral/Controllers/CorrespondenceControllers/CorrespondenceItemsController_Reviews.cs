using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using CommandCentral.Entities.Correspondence;
using CommandCentral.Enums;
using CommandCentral.Events;
using Microsoft.AspNetCore.Mvc;

namespace CommandCentral.Controllers.CorrespondenceControllers
{
    public partial class CorrespondenceItemsController
    {
        /// <summary>
        /// Gets the reviews collection of the given correspondence item.
        /// </summary>
        /// <param name="correspondenceItemId">Id of the correspondence item owning the reviews collection you wish to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{correspondenceItemId}/Reviews")]
        [ProducesResponseType(200, Type = typeof(List<DTOs.CorrespondenceReview.Get>))]
        public IActionResult GetReviews(Guid correspondenceItemId)
        {
            var item = DBSession.Get<CorrespondenceItem>(correspondenceItemId);
            if (item == null)
                return NotFoundParameter(correspondenceItemId, nameof(correspondenceItemId));

            if (!item.CanPersonViewItem(User))
                return Forbid();

            var result = DBSession.Query<CorrespondenceReview>()
                .Where(x => x.CorrespondenceItem == item)
                .Select(review => new DTOs.CorrespondenceReview.Get(review))
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Gets the review identified by the id and the correspondence item's id.
        /// </summary>
        /// <param name="correspondenceItemId">Id of the correspondence item owning the review you wish to retrieve.</param>
        /// <param name="reviewId">Id of the review itself.</param>
        /// <returns></returns>
        [HttpGet("{correspondenceItemId}/Reviews/{reviewId}")]
        [ProducesResponseType(200, Type = typeof(DTOs.CorrespondenceReview.Get))]
        public IActionResult GetReview(Guid correspondenceItemId, Guid reviewId)
        {
            var review = DBSession.Query<CorrespondenceReview>()
                .SingleOrDefault(x => x.CorrespondenceItem.Id == correspondenceItemId && x.Id == reviewId);

            if (review == null)
                return NotFoundChildParameter(correspondenceItemId, nameof(correspondenceItemId), reviewId,
                    nameof(reviewId));

            if (!review.CorrespondenceItem.CanPersonViewItem(User))
                return Forbid();

            return Ok(new DTOs.CorrespondenceReview.Get(review));
        }

        /// <summary>
        /// Creates a new review.  This is how you "route" a correspondence item to a new person, simply create a new review with that person as the reviewer.  
        /// You may not create a new review for a correspondence item if that item already has a review pending approval.  Additionally, if you create a new review  
        /// with IsFinal = true, the reviewer of that review must match the FinalApprover of the correspondence item.
        /// </summary>
        /// <param name="correspondenceItemId">Id of the correspondence item owning the reviews collection you wish to POST to.</param>
        /// <param name="dto">A dto containing all of the information needed to create a new review.</param>
        /// <returns></returns>
        [HttpPost("{correspondenceItemId}/Reviews")]
        [ProducesResponseType(201, Type = typeof(DTOs.CorrespondenceReview.Get))]
        public IActionResult PostReview(Guid correspondenceItemId, [FromBody] DTOs.CorrespondenceReview.Post dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var correspondenceItem = DBSession.Get<CorrespondenceItem>(correspondenceItemId);
            if (correspondenceItem == null)
                return NotFoundParameter(correspondenceItemId, nameof(correspondenceItemId));

            if (correspondenceItem.HasBeenCompleted)
                return Conflict(
                    "The correspondence item has been completed.  Further modifications of it and its reviews is no longer allowed.");

            if (!correspondenceItem.CanPersonEditItem(User))
                return Forbid();

            var reviewer = DBSession.Get<Person>(dto.Reviewer);
            if (reviewer == null)
                return NotFoundParameter(dto.Reviewer, nameof(dto.Reviewer));

            var review = new CorrespondenceReview
            {
                Id = Guid.NewGuid(),
                IsFinal = dto.IsFinal,
                Reviewer = reviewer,
                CorrespondenceItem = correspondenceItem,
                TimeRouted = CallTime,
                RoutedBy = User
            };
            correspondenceItem.Reviews.Add(review);

            //We need to do validation for both the corr item and the review.  This is because the addition of this review could violate rules on the parent corr item.
            var errors = review.Validate().Errors.Select(x => x.ErrorMessage)
                .Concat(correspondenceItem.Validate().Errors.Select(x => x.ErrorMessage));
            if (errors.Any())
                return BadRequest(errors);

            DBSession.Save(review);

            CommitChanges();

            EventManager.OnCorrespondenceRouted(new Events.Args.CorrespondenceItemRoutedEventArgs()
            {
                Item = correspondenceItem,
                NewPersonRoutedTo = review.Reviewer
            }, this);

            return CreatedAtAction(nameof(GetReview),
                new {correspondenceItemId = correspondenceItem.Id, reviewId = review.Id},
                new DTOs.CorrespondenceReview.Get(review));
        }

        /// <summary>
        /// Modifies an existing review.  This is how a client can recommended a review.  Reviews may by modified after recommendation, but you can never set the IsRecommended property back to null.  
        /// Recommending a review either positively or negatively will set the appropriate other fields.  Only the Reviewer or a person with access to Admin Tools may review a review.  
        /// If the client is the FinalApprover, submission of a recommendation will be considered 'Final' and will set the HasBeenCompleted property of the correspondence item to true.  
        /// If a final approver wishes to recycle a correspondence item to a different person, he or she should delete the review that was routed to them, and then route the correspondence item to a new person.
        /// </summary>
        /// <param name="correspondenceItemId">Id of the correspondence item owning the review you wish to modify.</param>
        /// <param name="reviewId">The id of the review you wish to modify.</param>
        /// <param name="dto">A dto containing all of the information required to modify a review.</param>
        /// <returns></returns>
        [HttpPut("{correspondenceItemId}/Reviews/{reviewId}")]
        [ProducesResponseType(201, Type = typeof(DTOs.CorrespondenceReview.Get))]
        public IActionResult PutReview(Guid correspondenceItemId, Guid reviewId,
            [FromBody] DTOs.CorrespondenceReview.Put dto)
        {
            if (dto == null)
                return BadRequestDTONull();

            var review = DBSession.Query<CorrespondenceReview>().SingleOrDefault(x =>
                x.CorrespondenceItem.Id == correspondenceItemId && x.Id == reviewId);
            if (review == null)
                return NotFoundChildParameter(correspondenceItemId, nameof(correspondenceItemId), reviewId,
                    nameof(reviewId));

            if (review.CorrespondenceItem.HasBeenCompleted)
                return Conflict(
                    "The correspondence item to which this review belongs has been completed.  Further modifications of it and its reviews is no longer allowed.");

            if (!User.CanAccessSubmodules(SpecialPermissions.AdminTools) && User != review.Reviewer)
                return Forbid();

            if (review.IsReviewed && !dto.IsRecommended.HasValue)
                return BadRequest(
                    "Once a review has been recommended either positively or negatively, it may not be 'unreviewed'.");

            review.IsRecommended = dto.IsRecommended;
            review.Body = dto.Body;
            if (!review.IsReviewed && review.IsRecommended.HasValue)
            {
                //Set the fields because the client just recommended the item.
                review.IsReviewed = true;
                review.ReviewedBy = User;
                review.TimeReviewed = CallTime;

                //Now that the client has done that, if the client is the final approver, then we also need set all of that up.
                if (review.CorrespondenceItem.FinalApprover == User)
                {
                    review.CorrespondenceItem.HasBeenCompleted = true;
                }
            }

            var result = review.Validate();
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(x => x.ErrorMessage));

            CommitChanges();

            EventManager.OnReviewModified(new Events.Args.CorrespondenceReviewEventArgs
            {
                Review = review
            }, this);

            if (review.CorrespondenceItem.HasBeenCompleted)
            {
                EventManager.OnCorrespondenceCompleted(new Events.Args.CorrespondenceItemEventArgs
                {
                    Item = review.CorrespondenceItem
                }, this);
            }

            return CreatedAtAction(nameof(GetReview),
                new {correspondenceItemId = review.CorrespondenceItem.Id, reviewId = review.Id},
                new DTOs.CorrespondenceReview.Get(review));
        }

        /// <summary>
        /// Deletes a review.  A review may not be deleted if the correspondence item has been completed.
        /// </summary>
        /// <param name="correspondenceItemId"></param>
        /// <param name="reviewId"></param>
        /// <returns></returns>
        [HttpDelete("{correspondenceItemId}/Reviews/{reviewId}")]
        [ProducesResponseType(204)]
        public IActionResult DeleteReview(Guid correspondenceItemId, Guid reviewId)
        {
            var review = DBSession.Query<CorrespondenceReview>().SingleOrDefault(x =>
                x.CorrespondenceItem.Id == correspondenceItemId && x.Id == reviewId);
            if (review == null)
                return NotFoundChildParameter(correspondenceItemId, nameof(correspondenceItemId), reviewId,
                    nameof(reviewId));

            if (review.CorrespondenceItem.HasBeenCompleted)
                return Conflict(
                    "The correspondence item to which this review belongs has been completed.  Further modifications of it and its reviews is no longer allowed.");

            if (!review.CorrespondenceItem.CanPersonEditItem(User))
                return Forbid();

            DBSession.Delete(review);

            CommitChanges();

            EventManager.OnReviewDeleted(new Events.Args.CorrespondenceReviewEventArgs
            {
                Review = review
            }, this);

            return Ok(new DTOs.CorrespondenceReview.Get(review));
        }
    }
}