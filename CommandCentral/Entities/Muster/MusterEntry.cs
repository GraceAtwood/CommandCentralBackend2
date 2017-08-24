using FluentNHibernate.Mapping;
using FluentValidation;
using System;
using FluentValidation.Results;

namespace CommandCentral.Entities.Muster
{
    public class MusterEntry : Entity
    {
        #region Properties

        /// <summary>
        /// The person for whom this muster entry was submitted.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// The person who submitted this muster entry.
        /// </summary>
        public virtual Person SubmittedBy { get; set; }

        /// <summary>
        /// The date/time at which this muster entry was submitted.
        /// </summary>
        public virtual DateTime TimeSubmitted { get; set; }

        /// <summary>
        /// The accountability type indicates how the person was mustered. (eg. Leave, present, etc.)
        /// </summary>
        public virtual ReferenceLists.AccountabilityType AccountabilityType { get; set; }

        /// <summary>
        /// The muster cycle to which this muster entry belongs.
        /// </summary>
        public virtual MusterCycle MusterCycle { get; set; }

        /// <summary>
        /// If a status period was used to preset this muster entry, then it will be found here.
        /// <para/>
        /// Note: If a muster entry is ever updated after it is set by a status period, then this property should be set to null as a status period no longer sets this muster entry.
        /// </summary>
        public virtual StatusPeriod StatusPeriodSetBy { get; set; }

        /// <summary>
        /// Archive information 
        /// </summary>
        public virtual MusterArchiveInformation ArchiveInformation { get; set; }

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
        public class MusterEntryMapping : ClassMap<MusterEntry>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public MusterEntryMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.TimeSubmitted).Not.Nullable();

                References(x => x.Person).Not.Nullable();
                References(x => x.SubmittedBy).Not.Nullable();
                References(x => x.AccountabilityType).Not.Nullable();
                References(x => x.MusterCycle).Not.Nullable();
                References(x => x.StatusPeriodSetBy);

                References(x => x.ArchiveInformation).Cascade.All();
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<MusterEntry>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Person).NotEmpty();
                RuleFor(x => x.SubmittedBy).NotEmpty();
                RuleFor(x => x.TimeSubmitted).NotEmpty();
                RuleFor(x => x.AccountabilityType).NotEmpty();
                RuleFor(x => x.MusterCycle).NotEmpty();
            }
        }
    }
}
