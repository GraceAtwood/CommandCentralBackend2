using CommandCentral.Utilities.Types;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace CommandCentral.Entities.Muster
{
    /// <summary>
    /// Encapsulates a single muster cycle (most likely a day) that extends from one date/time to another and holds all of the muster records submitted for that cycle.
    /// </summary>
    public class MusterCycle : Entity
    {
        #region Properties

        /// <summary>
        /// The range of this muster cycle.
        /// </summary>
        public virtual TimeRange Range { get; set; }

        /// <summary>
        /// Indicates this muster cycle has been finalized.  If true, no further muster entries may be submitted.
        /// </summary>
        public virtual bool IsFinalized { get; set; }

        /// <summary>
        /// The date/time at which this cycle was finalized.
        /// </summary>
        public virtual DateTime? TimeFinalized { get; set; }

        /// <summary>
        /// The person that finalized this cycle.
        /// </summary>
        public virtual Person FinalizedBy { get; set; }

        /// <summary>
        /// The command for which this muster cycle is conducted.
        /// </summary>
        public virtual Command Command { get; set; }

        /// <summary>
        /// The muster entries which account for the sailors at this command.
        /// </summary>
        public virtual IList<MusterEntry> MusterEntries { get; set; }

        /// <summary>
        /// Indicates that this muster cycle was finalized by the system automatically and not be any user.
        /// </summary>
        public virtual bool? WasFinalizedBySystem { get; set; }

        #endregion

        /// <summary>
        /// Validates this object.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }
        
        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class MusterCycleMapping : ClassMap<MusterCycle>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public MusterCycleMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.IsFinalized).Not.Nullable();
                Map(x => x.TimeFinalized).CustomType<UtcDateTimeType>();
                Map(x => x.WasFinalizedBySystem);

                Component(x => x.Range, map =>
                {
                    map.Map(x => x.End).Not.Nullable().CustomType<UtcDateTimeType>();
                    map.Map(x => x.Start).Not.Nullable().CustomType<UtcDateTimeType>();
                });

                References(x => x.FinalizedBy);
                References(x => x.Command);

                HasMany(x => x.MusterEntries).Cascade.All();
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<MusterCycle>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();

                RuleFor(x => x.Range)
                    .Must(range => range.Start <= range.End && range.Start != default(DateTime) && range.End != default(DateTime))
                        .WithMessage("A muster cycle must start before it ends.");

                When(x => x.IsFinalized, () =>
                {
                    RuleFor(x => x.WasFinalizedBySystem).NotEmpty();

                    When(x => x.WasFinalizedBySystem == true, () =>
                    {
                        RuleFor(x => x.FinalizedBy).Empty();
                    });

                    When(x => x.WasFinalizedBySystem == false, () =>
                    {
                        RuleFor(x => x.FinalizedBy).NotEmpty();
                    });

                    RuleFor(x => x.TimeFinalized).Must(x => x.HasValue && x.Value != default(DateTime));
                });

                RuleFor(x => x.Command).NotEmpty();
            }
        }
    }
}
