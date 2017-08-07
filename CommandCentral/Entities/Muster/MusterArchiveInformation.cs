using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using FluentNHibernate.Mapping;
using FluentValidation;

namespace CommandCentral.Entities.Muster
{
    /// <summary>
    /// Represents historical information derived from the muster system.  Intended to act as a daily snap shot of a sailor's most important fields.
    /// </summary>
    public class MusterArchiveInformation : Entity
    {
        #region Properties

        /// <summary>
        /// The person's command at the time they were mustered.
        /// </summary>
        public virtual string Command { get; set; }

        /// <summary>
        /// The person's department at the time they were mustered.
        /// </summary>
        public virtual string Department { get; set; }

        /// <summary>
        /// The person's division at the time they were mustered.
        /// </summary>
        public virtual string Division { get; set; }

        /// <summary>
        /// The person's designation at the time they were mustered.
        /// </summary>
        public virtual string Designation { get; set; }

        /// <summary>
        /// The person's paygrade at the time they were mustered.
        /// </summary>
        public virtual string Paygrade { get; set; }

        /// <summary>
        /// The person's UIC at the time they were mustered.
        /// </summary>
        public virtual string UIC { get; set; }

        /// <summary>
        /// The muster entry to which this archive belongs.
        /// </summary>
        public virtual MusterEntry MusterEntry { get; set; }

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
        public class MusterArchiveInformationMapping : ClassMap<MusterArchiveInformation>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public MusterArchiveInformationMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Command).Not.Nullable();
                Map(x => x.Department).Not.Nullable();
                Map(x => x.Division).Not.Nullable();
                Map(x => x.Designation).Not.Nullable();
                Map(x => x.Paygrade).Not.Nullable();
                Map(x => x.UIC).Not.Nullable();

                References(x => x.MusterEntry).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<MusterArchiveInformation>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Command).NotEmpty();
                RuleFor(x => x.Department).NotEmpty();
                RuleFor(x => x.Division).NotEmpty();
                RuleFor(x => x.Designation).NotEmpty();
                RuleFor(x => x.Paygrade).NotEmpty();
                RuleFor(x => x.UIC).NotEmpty();
            }
        }
    }
}
