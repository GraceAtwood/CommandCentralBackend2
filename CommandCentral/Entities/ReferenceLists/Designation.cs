using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using CommandCentral.Framework;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single designation.  This is the job title for civilians, the rate for enlisted and the designator for officers.
    /// </summary>
    [EditableReferenceList]
    public class Designation : ReferenceListItemBase
    {
        /// <summary>
        /// Validates this designation.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new DesignationValidator().Validate(this);
        }

        /// <summary>
        /// Maps a Designation to the database.
        /// </summary>
        public class DesignationMapping : SubclassMap<Designation>
        {
        }

        /// <summary>
        /// Validates a designation.
        /// </summary>
        public class DesignationValidator : AbstractValidator<Designation>
        {
            /// <summary>
            /// Validates a designation.
            /// </summary>
            public DesignationValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a designation can be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }
    }
}