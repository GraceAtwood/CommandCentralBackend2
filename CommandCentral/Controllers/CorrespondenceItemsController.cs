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
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] bool? hasAttachments, [FromQuery] string commentedBy,
            [FromQuery] bool? hasComments, [FromQuery] bool? hasReviews, [FromQuery] string reviewer, [FromQuery] string reviewedBy,
            [FromQuery] string sharedWith, [FromQuery] string finalApprover, [FromQuery] string pendingReviewer, [FromQuery] bool? hasBeenCompleted,
            [FromQuery] bool? hasPhyisicalCounterpart, [FromQuery] string body, [FromQuery] string type,
            [FromQuery] int limit = 1000, [FromQuery] string orderBy = nameof(CorrespondenceItem.TimeSubmitted))
        {
            if (limit <= 0)
                return BadRequest($"The value '{limit}' for the property '{nameof(limit)}' was invalid.  It must be greater than zero.");

            var query = DBSession.Query<CorrespondenceItem>();

            if (!String.IsNullOrWhiteSpace(seriesNumbers))
            {
                Expression<Func<CorrespondenceItem, bool>> predicate = null;

                foreach (var term in seriesNumbers.Split(',', ' ', '-', ';').Select(x => x.Trim()))
                {
                    if (!Int32.TryParse(term, out int number))
                        return BadRequest($"One or more terms given in your parameter '{nameof(seriesNumbers)}' were not valid.  They must all be integers.");

                    predicate = predicate.NullSafeOr(x => x.SeriesNumber == number);
                }

                query = query.Where(predicate);
            }

            if (!String.IsNullOrWhiteSpace(submittedFor))
            {
                Expression<Func<CorrespondenceItem, bool>> predicate = null;

                foreach (var phrase in submittedFor.Split(',').Select(x => x.Trim()))
                {
                    if (Guid.TryParse(phrase, out Guid id))
                    {
                        predicate = predicate.NullSafeOr(x => x.SubmittedFor.Id == id);
                    }
                    else
                    {
                        var terms = phrase.Split();
                        Expression<Func<CorrespondenceItem, bool>> subPredicate = null;

                        foreach (var term in phrase.Split())
                        {
                            subPredicate = subPredicate.NullSafeAnd(x =>
                                x.SubmittedFor.FirstName.Contains(term) ||
                                x.SubmittedFor.LastName.Contains(term) ||
                                x.SubmittedFor.MiddleName.Contains(term) ||
                                x.SubmittedFor.Division.Name.Contains(term) ||
                                x.SubmittedFor.Division.Department.Name.Contains(term) ||
                                x.SubmittedFor.Paygrade.Value.Contains(term) ||
                                x.SubmittedFor.UIC.Value.Contains(term) ||
                                x.SubmittedFor.Designation.Value.Contains(term));
                        }

                        predicate = predicate.NullSafeOr(subPredicate);
                    }
                }

                query = query.Where(predicate);
            }

            if (!String.IsNullOrWhiteSpace(submittedBy))
            {
                Expression<Func<CorrespondenceItem, bool>> predicate = null;

                foreach (var phrase in submittedBy.Split(',').Select(x => x.Trim()))
                {
                    if (Guid.TryParse(phrase, out Guid id))
                    {
                        predicate = predicate.NullSafeOr(x => x.SubmittedBy.Id == id);
                    }
                    else
                    {
                        var terms = phrase.Split();
                        Expression<Func<CorrespondenceItem, bool>> subPredicate = null;

                        foreach (var term in phrase.Split())
                        {
                            subPredicate = subPredicate.NullSafeAnd(x =>
                                x.SubmittedBy.FirstName.Contains(term) ||
                                x.SubmittedBy.LastName.Contains(term) ||
                                x.SubmittedBy.MiddleName.Contains(term) ||
                                x.SubmittedBy.Division.Name.Contains(term) ||
                                x.SubmittedBy.Division.Department.Name.Contains(term) ||
                                x.SubmittedBy.Paygrade.Value.Contains(term) ||
                                x.SubmittedBy.UIC.Value.Contains(term) ||
                                x.SubmittedBy.Designation.Value.Contains(term));
                        }

                        predicate = predicate.NullSafeOr(subPredicate);
                    }
                }

                query = query.Where(predicate);
            }

            if (!String.IsNullOrWhiteSpace(commentedBy))
            {
                Expression<Func<CorrespondenceItem, bool>> predicate = null;

                foreach (var phrase in commentedBy.Split(',').Select(x => x.Trim()))
                {
                    if (Guid.TryParse(phrase, out Guid id))
                    {
                        predicate = predicate.NullSafeOr(x => x.Comments.Any(y => y.Creator.Id == id));
                    }
                    else
                    {
                        var terms = phrase.Split();
                        Expression<Func<CorrespondenceItem, bool>> subPredicate = null;

                        foreach (var term in phrase.Split())
                        {
                            subPredicate = subPredicate.NullSafeAnd(x => x.Comments.Any(y =>
                                y.Creator.FirstName.Contains(term) ||
                                y.Creator.LastName.Contains(term) ||
                                y.Creator.MiddleName.Contains(term) ||
                                y.Creator.Division.Name.Contains(term) ||
                                y.Creator.Division.Department.Name.Contains(term) ||
                                y.Creator.Paygrade.Value.Contains(term) ||
                                y.Creator.UIC.Value.Contains(term) ||
                                y.Creator.Designation.Value.Contains(term)));
                        }

                        predicate = predicate.NullSafeOr(subPredicate);
                    }
                }

                query = query.Where(predicate);
            }

            if (!String.IsNullOrWhiteSpace(reviewer))
            {
                Expression<Func<CorrespondenceItem, bool>> predicate = null;

                foreach (var phrase in reviewer.Split(',').Select(x => x.Trim()))
                {
                    if (Guid.TryParse(phrase, out Guid id))
                    {
                        predicate = predicate.NullSafeOr(x => x.Reviews.Any(y => y.Reviewer.Id == id));
                    }
                    else
                    {
                        var terms = phrase.Split();
                        Expression<Func<CorrespondenceItem, bool>> subPredicate = null;

                        foreach (var term in phrase.Split())
                        {
                            subPredicate = subPredicate.NullSafeAnd(x => x.Reviews.Any(y =>
                                y.Reviewer.FirstName.Contains(term) ||
                                y.Reviewer.LastName.Contains(term) ||
                                y.Reviewer.MiddleName.Contains(term) ||
                                y.Reviewer.Division.Name.Contains(term) ||
                                y.Reviewer.Division.Department.Name.Contains(term) ||
                                y.Reviewer.Paygrade.Value.Contains(term) ||
                                y.Reviewer.UIC.Value.Contains(term) ||
                                y.Reviewer.Designation.Value.Contains(term)));
                        }

                        predicate = predicate.NullSafeOr(subPredicate);
                    }
                }

                query = query.Where(predicate);
            }

            if (!String.IsNullOrWhiteSpace(reviewedBy))
            {
                Expression<Func<CorrespondenceItem, bool>> predicate = null;

                foreach (var phrase in reviewedBy.Split(',').Select(x => x.Trim()))
                {
                    if (Guid.TryParse(phrase, out Guid id))
                    {
                        predicate = predicate.NullSafeOr(x => x.Reviews.Any(y => y.ReviewedBy.Id == id));
                    }
                    else
                    {
                        var terms = phrase.Split();
                        Expression<Func<CorrespondenceItem, bool>> subPredicate = null;

                        foreach (var term in phrase.Split())
                        {
                            subPredicate = subPredicate.NullSafeAnd(x => x.Reviews.Any(y =>
                                y.ReviewedBy.FirstName.Contains(term) ||
                                y.ReviewedBy.LastName.Contains(term) ||
                                y.ReviewedBy.MiddleName.Contains(term) ||
                                y.ReviewedBy.Division.Name.Contains(term) ||
                                y.ReviewedBy.Division.Department.Name.Contains(term) ||
                                y.ReviewedBy.Paygrade.Value.Contains(term) ||
                                y.ReviewedBy.UIC.Value.Contains(term) ||
                                y.ReviewedBy.Designation.Value.Contains(term)));
                        }

                        predicate = predicate.NullSafeOr(subPredicate);
                    }
                }

                query = query.Where(predicate);
            }

            if (!String.IsNullOrWhiteSpace(sharedWith))
            {
                Expression<Func<CorrespondenceItem, bool>> predicate = null;

                foreach (var phrase in sharedWith.Split(',').Select(x => x.Trim()))
                {
                    if (Guid.TryParse(phrase, out Guid id))
                    {
                        predicate = predicate.NullSafeOr(x => x.SharedWith.Any(y => y.Id == id));
                    }
                    else
                    {
                        var terms = phrase.Split();
                        Expression<Func<CorrespondenceItem, bool>> subPredicate = null;

                        foreach (var term in phrase.Split())
                        {
                            subPredicate = subPredicate.NullSafeAnd(x => x.SharedWith.Any(y =>
                                y.FirstName.Contains(term) ||
                                y.LastName.Contains(term) ||
                                y.MiddleName.Contains(term) ||
                                y.Division.Name.Contains(term) ||
                                y.Division.Department.Name.Contains(term) ||
                                y.Paygrade.Value.Contains(term) ||
                                y.UIC.Value.Contains(term) ||
                                y.Designation.Value.Contains(term)));
                        }

                        predicate = predicate.NullSafeOr(subPredicate);
                    }
                }

                query = query.Where(predicate);
            }

            if (!String.IsNullOrWhiteSpace(finalApprover))
            {
                Expression<Func<CorrespondenceItem, bool>> predicate = null;

                foreach (var phrase in finalApprover.Split(',').Select(x => x.Trim()))
                {
                    if (Guid.TryParse(phrase, out Guid id))
                    {
                        predicate = predicate.NullSafeOr(x => x.FinalApprover.Id == id);
                    }
                    else
                    {
                        var terms = phrase.Split();
                        Expression<Func<CorrespondenceItem, bool>> subPredicate = null;

                        foreach (var term in phrase.Split())
                        {
                            subPredicate = subPredicate.NullSafeAnd(x => 
                                x.FinalApprover.FirstName.Contains(term) ||
                                x.FinalApprover.LastName.Contains(term) ||
                                x.FinalApprover.MiddleName.Contains(term) ||
                                x.FinalApprover.Division.Name.Contains(term) ||
                                x.FinalApprover.Division.Department.Name.Contains(term) ||
                                x.FinalApprover.Paygrade.Value.Contains(term) ||
                                x.FinalApprover.UIC.Value.Contains(term) ||
                                x.FinalApprover.Designation.Value.Contains(term));
                        }

                        predicate = predicate.NullSafeOr(subPredicate);
                    }
                }

                query = query.Where(predicate);
            }

            if (!String.IsNullOrWhiteSpace(pendingReviewer))
            {
                Expression<Func<CorrespondenceItem, bool>> predicate = null;

                foreach (var phrase in pendingReviewer.Split(',').Select(x => x.Trim()))
                {
                    if (Guid.TryParse(phrase, out Guid id))
                    {
                        predicate = predicate.NullSafeOr(x => x.Reviews.Any(y => y.IsReviewed == false && y.Reviewer.Id == id));
                    }
                    else
                    {
                        var terms = phrase.Split();
                        Expression<Func<CorrespondenceItem, bool>> subPredicate = null;

                        foreach (var term in phrase.Split())
                        {
                            subPredicate = subPredicate.NullSafeAnd(x =>
                                x.FinalApprover.FirstName.Contains(term) ||
                                x.FinalApprover.LastName.Contains(term) ||
                                x.FinalApprover.MiddleName.Contains(term) ||
                                x.FinalApprover.Division.Name.Contains(term) ||
                                x.FinalApprover.Division.Department.Name.Contains(term) ||
                                x.FinalApprover.Paygrade.Value.Contains(term) ||
                                x.FinalApprover.UIC.Value.Contains(term) ||
                                x.FinalApprover.Designation.Value.Contains(term));
                        }

                        predicate = predicate.NullSafeOr(subPredicate);
                    }
                }

                query = query.Where(predicate);
            }

            if (!String.IsNullOrWhiteSpace(type))
            {
                Expression<Func<CorrespondenceItem, bool>> predicate = null;

                foreach (var phrase in type.Split(',').Select(x => x.Trim()))
                {
                    if (Guid.TryParse(phrase, out Guid id))
                    {
                        predicate = predicate.NullSafeOr(x => x.Id == id);
                    }
                    else
                    {
                        var terms = phrase.Split();
                        Expression<Func<CorrespondenceItem, bool>> subPredicate = null;

                        foreach (var term in terms)
                        {
                            subPredicate = subPredicate.NullSafeOr(x => x.Type.Value.Contains(term));
                        }

                        predicate = predicate.NullSafeOr(subPredicate);
                    }
                }

                query = query.Where(predicate);
            }

            if (!String.IsNullOrWhiteSpace(body))
            {
                Expression<Func<CorrespondenceItem, bool>> predicate = null;

                foreach (var phrase in body.Split(',').Select(x => x.Trim()))
                {
                    predicate = predicate.NullSafeOr(x => x.Body.Contains(phrase));
                }

                query = query.Where(predicate);
            }

            if (from.HasValue && !to.HasValue)
                query = query.Where(x => x.TimeSubmitted >= from);
            else if (to.HasValue && !from.HasValue)
                query = query.Where(x => x.TimeSubmitted <= to);
            else if (to.HasValue && to.HasValue)
                query = query.Where(x => x.TimeSubmitted <= to && x.TimeSubmitted >= from);

            if (hasAttachments.HasValue)
                query = query.Where(x => x.Attachments.Count() > 0);

            if (hasComments.HasValue)
                query = query.Where(x => x.Comments.Count() > 0);

            if (hasReviews.HasValue)
                query = query.Where(x => x.Reviews.Count() > 0);

            if (hasBeenCompleted.HasValue)
                query = query.Where(x => x.HasBeenCompleted == true);

            if (hasPhyisicalCounterpart.HasValue)
                query = query.Where(x => x.HasPhysicalCounterpart == true);

            //This query will add the permissions restrictions
            query = query.Where(x => x.SubmittedBy == User || x.SubmittedFor == User || x.Reviews.Any(y => y.Reviewer == User || y.ReviewedBy == User) || x.SharedWith.Contains(User));

            if (String.Equals(orderBy, nameof(CorrespondenceItem.TimeSubmitted), StringComparison.CurrentCultureIgnoreCase))
                query = query.OrderByDescending(x => x.TimeSubmitted);
            else
                return BadRequest($"Your requested value '{orderBy}' for the parameter '{nameof(orderBy)}' is not supported.  The supported values are '{nameof(CorrespondenceItem.TimeSubmitted)}' ... and nothing else.  :/  If you need other order by properties here, just tell me.");

            var result = query
                .Take(limit)
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
