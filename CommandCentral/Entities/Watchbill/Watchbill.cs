using System.Collections.Generic;
using CommandCentral.Enums;
using CommandCentral.Framework;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using Itenso.TimePeriod;

namespace CommandCentral.Entities.Watchbill
{
    public class Watchbill : Entity, IHazComments
    {
        /// <summary>
        /// The title of this watchbill
        /// </summary>
        public virtual string Title { get; set; }
        
        /// <summary>
        /// The month of this watchbill
        /// </summary>
        public virtual int Month { get; set; }
        
        /// <summary>
        /// The year of this watchbill
        /// </summary>
        public virtual int Year { get; set; }
        
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
        public virtual IList<Comment> Comments { get; set; }

        public virtual bool CanPersonAccessComments(Person person)
        {
            throw new System.NotImplementedException();
        }

        public class WatchbillMapping : ClassMap<Watchbill>
        {
            public WatchbillMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Title).Not.Nullable();
                Map(x => x.Month).Not.Nullable();
                Map(x => x.Year).Not.Nullable();
                Map(x => x.Phase).Not.Nullable();

                References(x => x.Command).Not.Nullable();

                HasMany(x => x.WatchShifts).Cascade.All();
            }
            
        }

        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Validates the watchbill
        /// </summary>
        public class Validator : AbstractValidator<Watchbill>
        {
            /// <summary>
            /// Validates the watchbill
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Title).NotEmpty().Length(3, 50);
                RuleFor(x => x.Month).NotEmpty().InclusiveBetween(1, 12);
                RuleFor(x => x.Year).NotEmpty().GreaterThan(2016);
                RuleFor(x => x.Command).NotEmpty();
            }
        }
    }
}