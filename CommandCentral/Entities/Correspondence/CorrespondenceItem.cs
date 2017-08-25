using CommandCentral.Framework.Data;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using CommandCentral.Framework;
using CommandCentral.Authorization;
using CommandCentral.Enums;

namespace CommandCentral.Entities.Correspondence
{
    /// <summary>
    /// A correspondence item describes and tracks the routing of paperwork either physically or digitally.
    /// </summary>
    public class CorrespondenceItem : Entity, IHazAttachments, IHazComments
    {
        #region Properties

        /// <summary>
        /// The series number of this item.  Intended to be printed on physical counterparts to tie them to the one in the system.
        /// </summary>
        public virtual int SeriesNumber { get; set; }

        /// <summary>
        /// The person for whom this item was submitted.  If a client submits an item for themselves, then the SubmittedFor and SubmittedBy properties will be equal.
        /// </summary>
        public virtual Person SubmittedFor { get; set; }

        /// <summary>
        /// The person who submitted this item.
        /// </summary>
        public virtual Person SubmittedBy { get; set; }

        /// <summary>
        /// The time at which this correspondence was submitted.
        /// </summary>
        public virtual DateTime TimeSubmitted { get; set; }

        /// <summary>
        /// The list of all attachments included in this correspondence.
        /// </summary>
        public virtual IList<FileAttachment> Attachments { get; set; } = new List<FileAttachment>();

        /// <summary>
        /// The list of comments for this item.
        /// </summary>
        public virtual IList<Comment> Comments { get; set; } = new List<Comment>();

        /// <summary>
        /// The list of all reviews that have been submitted for this correspondence.
        /// </summary>
        public virtual IList<CorrespondenceReview> Reviews { get; set; } = new List<CorrespondenceReview>();

        /// <summary>
        /// The list of persons this correspondence item has been shared with.
        /// </summary>
        public virtual IList<Person> SharedWith { get; set; } = new List<Person>();

        /// <summary>
        /// The person who is ultimately responsible for approving or denying this correspondence.
        /// </summary>
        public virtual Person FinalApprover { get; set; }

        /// <summary>
        /// Indicates if this item has been completed.  If it has been completed, then the final approver must have reviewed this item.
        /// </summary>
        public virtual bool HasBeenCompleted { get; set; }

        /// <summary>
        /// The type of the correspondence.  This is a reference list.
        /// </summary>
        public virtual ReferenceLists.CorrespondenceItemType Type { get; set; }

        /// <summary>
        /// The body of this item.  Just a free text field clients can use to write in.
        /// </summary>
        public virtual string Body { get; set; }

        /// <summary>
        /// Indicates that this correspondence item was routed with accompanying physical paperwork.
        /// </summary>
        public virtual bool HasPhysicalCounterpart { get; set; }

        /// <summary>
        /// This item's priority level.
        /// </summary>
        public virtual CorrespondenceItemPriorityLevels PriorityLevel { get; set; }

        #endregion

        /// <summary>
        /// Determines if a person can access the comments.  True if :
        /// return SubmittedBy == person || SubmittedFor == person || 
        ///        this.Reviews.Any(x => x.Reviewer == person || x.ReviewedBy == person);
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool CanPersonAccessComments(Person person)
        {
            return SubmittedBy == person || SubmittedFor == person || 
                Reviews.Any(x => x.Reviewer == person || x.ReviewedBy == person);
        }

        /// <summary>
        /// Determines if a person can access the attachments.  True if :
        /// return SubmittedBy == person || SubmittedFor == person || 
        ///        this.Reviews.Any(x => x.Reviewer == person || x.ReviewedBy == person);
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool CanPersonAccessAttachments(Person person)
        {
            return SubmittedBy == person || SubmittedFor == person ||
                Reviews.Any(x => x.Reviewer == person || x.ReviewedBy == person);
        }

        /// <summary>
        /// Determines if the given person can see this correspondence item.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool CanPersonViewItem(Person person)
        {
            if (CanPersonEditItem(person))
                return true;

            if (person.IsInChainOfCommand(SubmittedFor))
                return true;

            return false;
        }

        /// <summary>
        /// Determines if the given person can edit this correspondence item.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool CanPersonEditItem(Person person)
        {
            if (person.CanAccessSubmodules(SubModules.AdminTools))
                return true;

            if (SubmittedBy == person || SubmittedFor == person ||
                Reviews.Any(y => y.Reviewer == person || y.ReviewedBy == person) || SharedWith.Contains(person))
                return true;

            return false;
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class CorrespondenceItemMapping : ClassMap<CorrespondenceItem>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public CorrespondenceItemMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.SeriesNumber).Not.Nullable().Unique();
                Map(x => x.TimeSubmitted).Not.Nullable();
                Map(x => x.HasBeenCompleted).Not.Nullable();
                Map(x => x.HasPhysicalCounterpart).Not.Nullable();
                Map(x => x.Body).Length(1000);
                Map(x => x.PriorityLevel);

                References(x => x.SubmittedFor).Not.Nullable();
                References(x => x.SubmittedBy).Not.Nullable();
                References(x => x.FinalApprover).Not.Nullable();
                References(x => x.Type).Not.Nullable();

                HasMany(x => x.Comments)
                    .Cascade.AllDeleteOrphan()
                    .KeyColumn("OwningEntity_id")
                    .ForeignKeyConstraintName("none");

                HasMany(x => x.Attachments)
                    .Cascade.AllDeleteOrphan()
                    .KeyColumn("OwningEntity_id")
                    .ForeignKeyConstraintName("none");

                HasMany(x => x.Reviews)
                    .Cascade.AllDeleteOrphan();

                HasMany(x => x.SharedWith);
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<CorrespondenceItem>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();

                RuleFor(x => x.SeriesNumber).GreaterThan(0)
                    .Must((item, num) =>
                    {
                        if (SessionManager.GetCurrentSession().Query<CorrespondenceItem>().Count(x => x.Id != item.Id && x.SeriesNumber == num) != 0)
                            return false;

                        return true;
                    })
                    .WithMessage("That series number is not unique!");

                RuleFor(x => x.TimeSubmitted).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow);

                When(x => x.HasBeenCompleted, () =>
                {
                    RuleFor(x => x.Reviews)
                        .Must((item, reviews) => reviews.Any(x => x.IsReviewed && x.Reviewer == item.FinalApprover))
                            .WithMessage("You can not set a correspondence item to 'completed' unless the final approver has reviewed the item.");
                });

                RuleFor(x => x.Body).Length(0, 1000);

                RuleFor(x => x.SubmittedBy).NotEmpty();
                RuleFor(x => x.SubmittedFor).NotEmpty();
                RuleFor(x => x.FinalApprover).NotEmpty();
                RuleFor(x => x.Type).NotEmpty();

                //This rule will ensure that no more than one review is pending and that, if a review is the final approver, then the final approver from the item must match that person.
                RuleFor(x => x.Reviews).Must((item, reviews) =>
                {
                    var hasPending = false;
                    foreach (var review in reviews)
                    {
                        if (!review.IsReviewed)
                        {
                            if (hasPending)
                                return false;

                            hasPending = true;
                        }

                        if (review.IsFinal && item.FinalApprover != review.Reviewer)
                            return false;
                    }

                    return true;
                })
                .WithMessage("Only one review of a correspondence item may be pending review.  Additionally, in order for a review to be marked as final, the reviewer of that review must match the final approver of the correspondence item.");
            }
        }
    }
}
