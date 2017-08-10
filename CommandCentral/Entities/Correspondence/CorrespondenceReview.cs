using FluentNHibernate.Mapping;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace CommandCentral.Entities.Correspondence
{
    /// <summary>
    /// A correspondence review encapsulates any remarks a person wants to make and whether or not they approve or disapprove the related correspondence item.
    /// </summary>
    public class CorrespondenceReview : Entity
    {
        #region Properties

        /// <summary>
        /// The person who is responsible for reviewing the item.
        /// </summary>
        public virtual Person Reviewer { get; set; }

        /// <summary>
        /// The person who reviewed this item.  It it is not the same as the reviewer, then a person reviewed this item on the reviewer's behalf.
        /// </summary>
        public virtual Person ReviewedBy { get; set; }

        /// <summary>
        /// The time at which the reviewer was sent this item.
        /// </summary>
        public virtual DateTime TimeReceived { get; set; }

        /// <summary>
        /// The time at which the reviewer reviewed the item.
        /// </summary>
        public virtual DateTime? TimeReviewed { get; set; }

        /// <summary>
        /// Indicates if the reviewer has reviewed the item yet.
        /// </summary>
        public virtual bool IsReviewed { get; set; }

        /// <summary>
        /// Indicates if the reviewer recommended the item or not.  Null if it hasn't been reviewed yet.
        /// </summary>
        public virtual bool? IsRecommended { get; set; }

        /// <summary>
        /// Free text field for the reviewer to describe anything they want.
        /// </summary>
        public virtual string Body { get; set; }

        /// <summary>
        /// The next review in the series.  Will be null if IsFinal is true.
        /// </summary>
        public virtual CorrespondenceReview NextReview { get; set; }

        /// <summary>
        /// The corr item that this review belongs to.
        /// </summary>
        public virtual CorrespondenceItem CorrespondenceItem { get; set; }

        /// <summary>
        /// Indicates that this review is the final review in the series of reviews.
        /// </summary>
        public virtual bool IsFinal { get; set; }

        #endregion

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
        public class CorrespondenceReviewMapping : ClassMap<CorrespondenceReview>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public CorrespondenceReviewMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.IsReviewed).Not.Nullable();
                Map(x => x.IsRecommended);
                Map(x => x.Body).Length(1000);
                Map(x => x.TimeReceived).Not.Nullable();
                Map(x => x.TimeReviewed);
                Map(x => x.IsFinal).Not.Nullable().Default(false.ToString());

                References(x => x.Reviewer).Not.Nullable();
                References(x => x.ReviewedBy);
                References(x => x.NextReview);
                References(x => x.CorrespondenceItem).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<CorrespondenceReview>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();

                RuleFor(x => x.Body).Length(0, 1000);

                RuleFor(x => x.TimeReceived).NotEmpty();

                RuleFor(x => x.Reviewer).NotEmpty();
                RuleFor(x => x.CorrespondenceItem).NotEmpty();

                When(x => x.IsReviewed, () =>
                {
                    RuleFor(x => x.IsRecommended).NotEmpty();
                    RuleFor(x => x.ReviewedBy).NotEmpty();
                    RuleFor(x => x.TimeReviewed).NotEmpty();

                    When(x => !x.IsFinal, () =>
                    {
                        RuleFor(x => x.NextReview).NotEmpty();
                    });
                });
            }
        }
    }
}
