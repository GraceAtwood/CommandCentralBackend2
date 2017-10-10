using CommandCentral.Framework;
using FluentNHibernate.Mapping;
using FluentValidation.Results;
using Itenso.TimePeriod;

namespace CommandCentral.Entities.Watchbill
{
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
        
        // Not including division or points assigned to because I think we don't need them.
        
        #endregion
        
        public class WatchShiftMapping : ClassMap<WatchShift>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchShiftMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Title).Not.Nullable();
                Map(x => x.Range).Not.Nullable();

                References(x => x.Watchbill).Not.Nullable();
                References(x => x.ShiftType).Not.Nullable();
                References(x => x.WatchAssignment);
            }
        }
        
        public override ValidationResult Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}