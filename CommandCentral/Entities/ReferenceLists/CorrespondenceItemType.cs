using CommandCentral.Framework;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Identifies different types of correspondence.
    /// </summary>
    [EditableReferenceList]
    public class CorrespondenceItemType : ReferenceListItemBase
    {
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
        public class CorrespondenceItemTypeMapping : ClassMap<CorrespondenceItemType>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public CorrespondenceItemTypeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<CorrespondenceItemType>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a correspondence item type must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be null.");
            }
        }
    }
}
