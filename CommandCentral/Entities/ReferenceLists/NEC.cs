using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using CommandCentral.Framework;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single NEC.
    /// </summary>
    [EditableReferenceList]
    public class NEC : ReferenceListItemBase
    {
        /// <summary>
        /// Validates an NEC.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Maps an NEC to the database.
        /// </summary>
        public class NECMapping : ClassMap<NEC>
        {
            /// <summary>
            /// Maps an NEC to the database.
            /// </summary>
            public NECMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates an NEC
        /// </summary>
        public class Validator : AbstractValidator<NEC>
        {
            /// <summary>
            /// Validates an NEC
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of an NEC can be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }
    }
}
