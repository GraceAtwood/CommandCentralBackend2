using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single Division.
    /// </summary>
    public class Division : Entity
    {

        #region Properties

        /// <summary>
        /// The name of this division.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// A brief description of this division.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The department to which this division belongs.
        /// </summary>
        public virtual Department Department { get; set; }

        #endregion

        #region Overrides
        
        /// <summary>
        /// Validates this division object.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        #endregion

        /// <summary>
        /// Maps a division to the database.
        /// </summary>
        public class DivisionMapping : ClassMap<Division>
        {
            /// <summary>
            /// Maps a division to the database.
            /// </summary>
            public DivisionMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Name).Not.Nullable().Unique();
                Map(x => x.Description);

                References(x => x.Department);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates le division.
        /// </summary>
        public class Validator : AbstractValidator<Division>
        {
            /// <summary>
            /// Validates the division.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a Department must be no more than 255 characters.");
                RuleFor(x => x.Name).NotEmpty()
                    .WithMessage("The value must not be empty");
            }
        }
    }
}
