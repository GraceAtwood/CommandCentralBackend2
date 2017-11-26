using CommandCentral.Utilities.Types;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using NHibernate.Type;

namespace CommandCentral.Entities.Watchbill
{
    /// <summary>
    /// A watch shift represents a single time slot dueing the month dueing which a watch stander is needed.
    /// </summary>
    public class WatchShift : Entity
    {
        #region Properties
        
        /// <summary>
        /// The Watchbill this shift belongs to
        /// </summary>
        public virtual Watchbill Watchbill { get; set; }
        
        /// <summary>
        /// The title of this shift
        /// </summary>
        public virtual string Title { get; set; }
        
        /// <summary>
        /// The start and end of this shift
        /// </summary>
        public virtual TimeRange Range { get; set; }
        
        /// <summary>
        /// The object describing the status of this shift's assignment
        /// </summary>
        public virtual WatchAssignment WatchAssignment { get; set; }
        
        /// <summary>
        /// The type of shift
        /// </summary>
        public virtual WatchShiftType ShiftType { get; set; }

        /// <summary>
        /// The division that is responsible for filling this requirement.
        /// </summary>
        public virtual Division DivisionAssignedTo { get; set; }

        #endregion
        
        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchShiftMapping : ClassMap<WatchShift>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchShiftMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Title).Not.Nullable();
                
                Component(x => x.Range, map =>
                {
                    map.Map(x => x.Start).Not.Nullable().CustomType<UtcDateTimeType>();
                    map.Map(x => x.End).Not.Nullable().CustomType<UtcDateTimeType>();
                });

                References(x => x.Watchbill).Not.Nullable();
                References(x => x.ShiftType).Not.Nullable();
                References(x => x.WatchAssignment);
                References(x => x.DivisionAssignedTo).Not.Nullable();
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
        /// Validates the WatchShift
        /// </summary>
        public class Validator : AbstractValidator<WatchShift>
        {
            /// <summary>
            /// Validates the WatchShift
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Title).NotEmpty().Length(3, 25);
                RuleFor(x => x.Watchbill).NotNull();
                RuleFor(x => x.ShiftType).NotNull();

                RuleFor(x => x).Must(x => x.Watchbill != null && 
                                          x.Watchbill.Year == x.Range.Start.Year &&
                                          x.Watchbill.Month == x.Range.Start.Month)
                                .WithMessage("The watch shift must be within the month of the watchbill.");
            }
        }
    }
}