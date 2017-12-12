using CommandCentral.Authorization;
using CommandCentral.Framework;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single physical address
    /// </summary>
    public class PhysicalAddress : Entity
    {

        #region Properties

        /// <summary>
        /// The street number + route address.
        /// </summary>
        public virtual string Address { get; set; }

        /// <summary>
        /// The city.
        /// </summary>
        public virtual string City { get; set; }

        /// <summary>
        /// The state.
        /// </summary>
        public virtual string State { get; set; }

        /// <summary>
        /// The zip code.
        /// </summary>
        public virtual string ZipCode { get; set; }

        /// <summary>
        /// The country.
        /// </summary>
        public virtual string Country { get; set; }

        /// <summary>
        /// Indicates whether or not the person lives at this address
        /// </summary>
        public virtual bool IsHomeAddress { get; set; }

        /// <summary>
        /// Indicates whether or not a person is ok with releasing this physica address outside their chain of command.
        /// </summary>
        public virtual bool IsReleasableOutsideCoC { get; set; }

        /// <summary>
        /// The person who owns this physical address.
        /// </summary>
        public virtual Person Person { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the address in this format: 123 Fake Street, Happyville, TX 54321
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{(IsHomeAddress ? "(Home) " : "")}{Address}, {City}, {State} {ZipCode}";
        }

        #endregion

        /// <summary>
        /// Returns a valdidation result for this object.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Maps a physical address to the database.
        /// </summary>
        public class PhysicalAddressMapping : ClassMap<PhysicalAddress>
        {
            /// <summary>
            /// Maps a physical address to the database.
            /// </summary>
            public PhysicalAddressMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Address).Not.Nullable();
                Map(x => x.City).Not.Nullable();
                Map(x => x.State).Not.Nullable();
                Map(x => x.ZipCode).Not.Nullable();
                Map(x => x.Country);
                Map(x => x.IsHomeAddress).Not.Nullable();
                Map(x => x.IsReleasableOutsideCoC).Not.Nullable();

                References(x => x.Person).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates a physical address
        /// </summary>
        public class Validator : AbstractValidator<PhysicalAddress>
        {
            /// <summary>
            /// Validates a physical address
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Person).NotEmpty();
                
                RuleFor(x => x.Address)
                    .NotEmpty().WithMessage("Your address must not be empty.")
                    .Length(1, 255).WithMessage("The address must be between 1 and 255 characters.");

                RuleFor(x => x.City)
                    .NotEmpty().WithMessage("Your city must not be empty.")
                    .Length(1, 255).WithMessage("The city must be between 1 and 255 characters.");

                RuleFor(x => x.State)
                    .NotEmpty().WithMessage("Your state must not be empty.")
                    .Length(1, 255).WithMessage("The state must be between 1 and 255 characters.");

                RuleFor(x => x.Country)
                    .Length(0, 255).WithMessage("The country may be no more than 200 characters.");

                RuleFor(x => x.ZipCode)
                    .NotEmpty().WithMessage("You zip code must not be empty.")
                    .Matches(@"^\d{5}(?:[-\s]\d{4})?$").WithMessage("Your zip code was not valid.");
            }
        }

        public class Contract : RulesContract<PhysicalAddress>
        {
            public Contract()
            {
                CanEditRuleOverride = (person, address) => person.IsInChainOfCommand(address.Person) || person == address.Person;
                CanReturnRuleOverride = (person, address) =>
                {
                    if (address.IsReleasableOutsideCoC)
                        return true;

                    return person.IsInChainOfCommand(address.Person) || person == address.Person;
                };
            }
        }
    }
}
