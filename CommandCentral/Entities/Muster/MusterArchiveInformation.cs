using System;
using CommandCentral.Framework;
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

        public MusterArchiveInformation()
        {

        }

        /// <summary>
        /// Creates a new muster archive information object, setting the Id.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="parent"></param>
        public MusterArchiveInformation(Person person, MusterEntry parent)
        {
            if (person == null)
                throw new ArgumentNullException(nameof(person));

            Command = person.Division.Department.Command?.Name;
            Department = person.Division.Department?.Name;
            Designation = person.Designation?.Value;
            Division = person.Division?.Name;
            Id = Guid.NewGuid();
            MusterEntry = parent ?? throw new ArgumentNullException(nameof(parent));
            Paygrade = person.Paygrade.ToString();
            UIC = person.UIC?.Value;
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
        public class MusterArchiveInformationMapping : ClassMap<MusterArchiveInformation>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public MusterArchiveInformationMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Command);
                Map(x => x.Department);
                Map(x => x.Division);
                Map(x => x.Designation);
                Map(x => x.Paygrade);
                Map(x => x.UIC);

                References(x => x.MusterEntry).Not.Nullable().Unique();
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
