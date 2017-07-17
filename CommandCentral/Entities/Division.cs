using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Criterion;
using CommandCentral.Framework;
using FluentValidation.Results;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Division.
    /// </summary>
    public class Division : Entity, IValidatable
    {

        #region Properties

        public virtual string Value { get; set; }

        public virtual string Description { get; set; }

        /// <summary>
        /// The department to which this division belongs.
        /// </summary>
        public virtual Department Department { get; set; }

        #endregion

        #region Helper Methods
        
        /// <summary>
        /// Validates this division object.
        /// </summary>
        /// <returns></returns>
        public virtual ValidationResult Validate()
        {
            return new DivisionValidator().Validate(this);
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

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                References(x => x.Department);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates le division.
        /// </summary>
        public class DivisionValidator : AbstractValidator<Division>
        {
            /// <summary>
            /// Validates the division.
            /// </summary>
            public DivisionValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a Department must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty");
            }
        }
    }
}
