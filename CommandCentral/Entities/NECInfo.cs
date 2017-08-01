using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using FluentNHibernate.Mapping;
using FluentValidation;
using CommandCentral.Entities.ReferenceLists;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Provides a wrapper around an NEC in order to indicate if an NEC is primary or secondary.
    /// </summary>
    public class NECInfo : Entity
    {
        #region Properties

        /// <summary>
        /// Indicates that the NEC referenced is a person's primary NEC.
        /// </summary>
        public virtual bool IsPrimary { get; set; }

        /// <summary>
        /// The NEC itself.
        /// </summary>
        public virtual NEC Nec { get; set; }

        /// <summary>
        /// The person to whom this nec info belongs.
        /// </summary>
        public virtual Person Person { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns ({(IsPrimary ? "Primary" : "Secondary")}) {Nec}
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"({(IsPrimary ? "Primary" : "Secondary")}) {Nec}";
        }

        #endregion

        /// <summary>
        /// Validates this object.
        /// </summary>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class NECInfoMapping : ClassMap<NECInfo>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public NECInfoMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.IsPrimary).Not.Nullable();

                References(x => x.Nec).Not.Nullable();
                References(x => x.Person).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<NECInfo>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Nec).NotEmpty()
                    .Must(x => ReferenceListHelper<NEC>.IdExists(x.Id));

                RuleFor(x => x.Person).NotEmpty();
            }
        }
    }
}
