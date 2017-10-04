using System;
using System.Data;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;

namespace CommandCentral.Entities.CFS
{
    /// <summary>
    /// Represents a single meeting held between an advisor and a person in order to satisfy the linked request.
    /// </summary>
    public class Meeting : Entity
    {
        #region Properties

        /// <summary>
        /// The time at which this meeting was held.
        /// </summary>
        public virtual DateTime Time { get; set; }

        /// <summary>
        /// The person who this meeting was held for.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// The person within the CFS group that conducted this meeting.
        /// </summary>
        public virtual Person Advisor { get; set; }

        /// <summary>
        /// Any notes the advisor chooses to make.
        /// </summary>
        public virtual string Notes { get; set; }

        /// <summary>
        /// The request this meeting was held for.
        /// </summary>
        public virtual Request Request { get; set; }

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
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<Meeting>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Time).NotEmpty();
                RuleFor(x => x.Person).NotEmpty();
                RuleFor(x => x.Advisor).NotEmpty();
                RuleFor(x => x.Notes).Length(0, 1000);
                RuleFor(x => x.Request).NotEmpty();
            }
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class MeetingMapping : ClassMap<Meeting>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public MeetingMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Time).Not.Nullable();
                Map(x => x.Notes).Length(1000);

                References(x => x.Person).Not.Nullable();
                References(x => x.Advisor).Not.Nullable();
                References(x => x.Request).Not.Nullable();
            }
        }
    }
}