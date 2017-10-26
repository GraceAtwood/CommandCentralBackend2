using CommandCentral.Enums;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;

namespace CommandCentral.Entities.Watchbill
{
    public class WatchShiftType : Entity
    {
        /// <summary>
        /// The Name of this shift
        /// </summary>
        public virtual string Name { get; set; }
        
        /// <summary>
        /// The optional description of this shift
        /// </summary>
        public virtual string Description { get; set; }
        
        /// <summary>
        /// The qualification required to stand this shift
        /// </summary>
        public virtual WatchQualifications Qualification { get; set; }
        
        public class WatchShiftTypeMapping : ClassMap<WatchShiftType>
        {
            public WatchShiftTypeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Name).Not.Nullable();
                Map(x => x.Description);
                Map(x => x.Qualification).CustomType<GenericEnumMapper<WatchQualifications>>();
            }
        }
        
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