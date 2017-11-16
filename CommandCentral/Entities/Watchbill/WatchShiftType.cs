using CommandCentral.Enums;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;

namespace CommandCentral.Entities.Watchbill
{
    /// <summary>
    /// Represents the type of a shift and ties it to a watch qualification.
    /// </summary>
    public class WatchShiftType : Entity
    {
        /// <summary>
        /// The name of this shift type.
        /// </summary>
        public virtual string Name { get; set; }
        
        /// <summary>
        /// The optional description of this shift type.
        /// </summary>
        public virtual string Description { get; set; }
        
        /// <summary>
        /// The qualification required to stand a shift marked with this shift type.
        /// </summary>
        public virtual WatchQualifications Qualification { get; set; }
        
        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchShiftTypeMapping : ClassMap<WatchShiftType>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchShiftTypeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Name).Not.Nullable();
                Map(x => x.Description);
                Map(x => x.Qualification).CustomType<GenericEnumMapper<WatchQualifications>>();
            }
        }
        
        /// <summary>
        /// Validates this object.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Validates the WatchShiftType
        /// </summary>
        public class Validator : AbstractValidator<WatchShiftType>
        {
            /// <summary>
            /// Validates the WatchShiftType
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty().Length(3, 20);
                RuleFor(x => x.Description).Length(0, 200);
                RuleFor(x => x.Qualification).NotNull();
            }
        }
    }
}