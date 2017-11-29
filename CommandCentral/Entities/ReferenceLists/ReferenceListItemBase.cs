using CommandCentral.Framework;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Provides abstracted access to a reference list such as Ranks or Rates.
    /// </summary>
    public abstract class ReferenceListItemBase : Entity
    {
        #region Properties

        /// <summary>
        /// The value of this item.
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// A description of this item.
        /// </summary>
        public virtual string Description { get; set; }

        #endregion

        /// <summary>
        /// Validates all reference lists if that reference list did not override the Validate method.
        /// </summary>
        public override ValidationResult Validate()
        {
            return new BaseValidator().Validate(this);
        }

        /// <summary>
        /// Validates all reference lists if that reference list did not override the Validate method.
        /// </summary>
        public class BaseValidator : AbstractValidator<ReferenceListItemBase>
        {
            /// <summary>
            /// Validates all reference lists if that reference list did not override the Validate method.
            /// </summary>
            public BaseValidator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("A reference list's description may be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }

        /// <summary>
        /// Maps all reference lists to the database.
        /// </summary>
        public class ReferenceListItemBaseMap : ClassMap<ReferenceListItemBase>
        {
            /// <summary>
            /// Maps all reference lists to the database.
            /// </summary>
            public ReferenceListItemBaseMap()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();

                UseUnionSubclassForInheritanceMapping();
            }
        }
    }
}