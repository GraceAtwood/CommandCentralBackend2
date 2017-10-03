using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using CommandCentral.Framework;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single UIC.
    /// </summary>
    public class UIC : ReferenceListItemBase
    {
        /// <summary>
        /// Returns a validation result which contains the result of validation. lol.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new UICValidator().Validate(this);
        }

        /// <summary>
        /// Maps a UIC to the database.
        /// </summary>
        public class UICMapping : SubclassMap<UIC>
        {
        }

        /// <summary>
        /// Validates the UIC.
        /// </summary>
        public class UICValidator : AbstractValidator<UIC>
        {
            /// <summary>
            /// Validates the UIC.
            /// </summary>
            public UICValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a UIC must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be null.");
            }
        }
    }
}
