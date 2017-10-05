using System;
using CommandCentral.Entities.ReferenceLists;
using FluentNHibernate.Mapping;
using FluentValidation;
using System.Linq;
using CommandCentral.Enums;
using FluentValidation.Results;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single Phone number along with its data access members and properties
    /// </summary>
    public class PhoneNumber : Entity
    {
        #region Properties

        /// <summary>
        /// The actual phone number of this phone number object.
        /// </summary>
        public virtual string Number { get; set; }

        /// <summary>
        /// Indicates whether or not a person is ok with releasing this phone number outside their chain of command.
        /// </summary>
        public virtual bool IsReleasableOutsideCoC { get; set; }

        /// <summary>
        /// Indicates whether or not the person who owns this phone number prefers contact to occur on it.
        /// </summary>
        public virtual bool IsPreferred { get; set; }

        /// <summary>
        /// The type of this phone. eg. Mobile, Home, Work
        /// </summary>
        public virtual PhoneNumberTypes PhoneType { get; set; }

        /// <summary>
        /// The person who owns this phone number.
        /// </summary>
        public virtual Person Person { get; set; }

        #endregion

        /// <summary>
        /// Returns a validation result for this object.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Maps a single phone number to the database.
        /// </summary>
        public class PhoneNumberMapping : ClassMap<PhoneNumber>
        {
            /// <summary>
            /// Maps a single phone number to the database.
            /// </summary>
            public PhoneNumberMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Number).Not.Nullable().Length(15);
                Map(x => x.IsReleasableOutsideCoC).Not.Nullable();
                Map(x => x.IsPreferred).Not.Nullable();
                Map(x => x.PhoneType).Not.Nullable();

                References(x => x.Person).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates the phone number object.
        /// </summary>
        public class Validator : AbstractValidator<PhoneNumber>
        {
            /// <summary>
            /// Validates the phone number object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Number).Length(0, 10)
                    .Must(x => x.All(char.IsDigit))
                    .WithMessage("Your phone number must only be 10 digits.");

                RuleFor(x => x.PhoneType).NotEmpty()
                    .WithMessage("The phone number type must not be left blank.");
            }
        }

    }
}
