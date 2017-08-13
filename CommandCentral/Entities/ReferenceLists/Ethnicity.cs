using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using FluentValidation;
using System.Linq;
using NHibernate.Criterion;
using CommandCentral.Framework;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single ethnicity
    /// </summary>
    [EditableReferenceList]
    public class Ethnicity : ReferenceListItemBase
    {
        /// <summary>
        /// Validates this ethnicity.
        /// </summary>
        /// <returns></returns>
        public override FluentValidation.Results.ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Maps an ethnicity to the database.
        /// </summary>
        public class EthnicityMapping : ClassMap<Ethnicity>
        {
            /// <summary>
            /// Maps an ethnicity to the database.
            /// </summary>
            public EthnicityMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates an ethnicity.
        /// </summary>
        public class Validator : AbstractValidator<Ethnicity>
        {
            /// <summary>
            /// Validates an ethnicity.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of an ethnicity may be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }
    }
}
