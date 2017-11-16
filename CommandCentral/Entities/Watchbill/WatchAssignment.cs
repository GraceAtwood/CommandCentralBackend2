using System;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;

namespace CommandCentral.Entities.Watchbill
{
    /// <summary>
    /// A watch assignment represents an assignment to a shift, encapsulating relevant information to that assignment.
    /// </summary>
    public class WatchAssignment : Entity
    {
        /// <summary>
        /// The watch shift that this assignment assigns the person to.
        /// </summary>
        public virtual WatchShift WatchShift { get; set; }

        /// <summary>
        /// The person that this watch assignment assigns to a watch shift.
        /// </summary>
        public virtual Person PersonAssigned { get; set; }

        /// <summary>
        /// The person who assigned the assigned person to the watch shift.  
        /// Basically, the person who created this assignment.
        /// </summary>
        public virtual Person AssignedBy { get; set; }

        /// <summary>
        /// The person who acknowledged this watch assignment.  
        /// Either the person assigned or someone who did it on their behalf.
        /// </summary>
        public virtual Person AcknowledgedBy { get; set; }
        
        /// <summary>
        /// The datetime at which this assignment was created.
        /// </summary>
        public virtual DateTime DateAssigned { get; set; }

        /// <summary>
        /// The datetime at which a person acknowledged this watch assignment.
        /// </summary>
        public virtual DateTime? DateAcknowledged { get; set; }

        /// <summary>
        /// Indicates if this watch assignment has been acknowledged.
        /// </summary>
        public virtual bool IsAcknowledged { get; set; }
        
        /// <summary>
        /// This is the number of times we've alerted the person assigned that they have watch.
        /// </summary>
        public virtual int NumberOfAlertsSent { get; set; }
        
        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchAssignmentMapping : ClassMap<WatchAssignment>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchAssignmentMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.DateAcknowledged);
                Map(x => x.DateAssigned).Not.Nullable();
                Map(x => x.IsAcknowledged);
                Map(x => x.NumberOfAlertsSent).Not.Nullable().Default("0");

                References(x => x.AcknowledgedBy);
                References(x => x.AssignedBy).Not.Nullable();
                References(x => x.PersonAssigned).Not.Nullable();
                References(x => x.WatchShift).Not.Nullable();
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
        /// Validates the WatchAssignment
        /// </summary>
        public class Validator : AbstractValidator<WatchAssignment>
        {
            /// <summary>
            /// Validates the WatchAssignment
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.AssignedBy).NotEmpty();
                RuleFor(x => x.PersonAssigned).NotEmpty();
                RuleFor(x => x.WatchShift).NotEmpty();
            }
        }
    }
}