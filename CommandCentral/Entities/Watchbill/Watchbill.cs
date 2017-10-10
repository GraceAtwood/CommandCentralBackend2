using System.Collections.Generic;
using CommandCentral.Enums;
using CommandCentral.Framework;
using FluentNHibernate.Mapping;
using FluentValidation.Results;
using Itenso.TimePeriod;

namespace CommandCentral.Entities.Watchbill
{
    public class Watchbill : Entity, IHazComments
    {
        /// <summary>
        /// The yonth of this watchbill
        /// </summary>
        public virtual Month Month { get; set; }
        
        /// <summary>
        /// The year of this watchbill
        /// </summary>
        public virtual Year Year { get; set; }
        
        /// <summary>
        /// The shifts contained in this watchbill
        /// </summary>
        public virtual IList<WatchShift> WatchShifts { get; set; }
        
        /// <summary>
        /// The command this watchbill is for
        /// </summary>
        public virtual Command Command { get; set; }
        
        /// <summary>
        /// The current phase of the watchbill
        /// </summary>
        public virtual WatchbillPhases Phase { get; set; }

        /// <summary>
        /// Any comments made on this watchbill
        /// </summary>
        public IList<Comment> Comments { get; set; }

        public bool CanPersonAccessComments(Person person)
        {
            throw new System.NotImplementedException();
        }

        public class WatchbillMapping : ClassMap<Watchbill>
        {
            public WatchbillMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Month).Not.Nullable();
                Map(x => x.Year).Not.Nullable();
                Map(x => x.Phase).Not.Nullable();

                References(x => x.Command).Not.Nullable();

                HasMany(x => x.WatchShifts).Cascade.All();
            }
            
        }

        public override ValidationResult Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}