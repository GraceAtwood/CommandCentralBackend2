using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using CommandCentral.Utilities.Types;
using CommandCentral.Events;
using CommandCentral.Events.Args;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single command, such as NIOC GA and all of its departments and divisions.
    /// </summary>
    public class Command : Entity
    {
        #region Properties

        /// <summary>
        /// The name of this command.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// A brief description of this command.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The departments of the command
        /// </summary>
        public virtual IList<Department> Departments { get; set; } = new List<Department>();

        /// <summary>
        /// The address of this command.
        /// </summary>
        public virtual string Address { get; set; }

        /// <summary>
        /// The city of this command.
        /// </summary>
        public virtual string City { get; set; }

        /// <summary>
        /// The state of this command.
        /// </summary>
        public virtual string State { get; set; }

        /// <summary>
        /// The command's zip code.
        /// </summary>
        public virtual string ZipCode { get; set; }

        /// <summary>
        /// The command's country.
        /// </summary>
        public virtual string Country { get; set; }

        /// <summary>
        /// The command's current muster cycle.
        /// </summary>
        public virtual Muster.MusterCycle CurrentMusterCycle { get; set; } 

        /// <summary>
        /// The hour of the day at which the muster begins.  This is also the same hour that, after 24 hours, the muster will rollover and finalize if it hasn't already been.
        /// </summary>
        public virtual int MusterStartHour { get; set; }

        /// <summary>
        /// The time zone in which this command resides.
        /// </summary>
        public virtual string TimeZoneId { get; set; }

        #endregion

        /// <summary>
        /// Validates this command object.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Gets the time zone info of this command.
        /// </summary>
        /// <returns></returns>
        public virtual TimeZoneInfo GetTimeZoneInfo()
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
        
        #region Muster Handling

        /// <summary>
        /// Rolls over the current muster cycle, finalizing the muser cycle if needed.
        /// </summary>
        /// <param name="person"></param>
        public virtual void RolloverCurrentMusterCycle(Person person)
        {
            if (!CurrentMusterCycle.IsFinalized)
            {
                //TODO
                //CurrentMusterCycle.FinalizeMusterCycle(person);
                EventManager.OnMusterFinalized(new MusterCycleEventArgs
                {
                    MusterCycle = CurrentMusterCycle
                }, this);
            }

            DateTime startTime;
            startTime = DateTime.UtcNow.Hour < MusterStartHour 
                ? DateTime.UtcNow.Date.AddDays(-1).AddHours(MusterStartHour) 
                : DateTime.UtcNow.Date.AddHours(MusterStartHour);

            CurrentMusterCycle = new Muster.MusterCycle
            {
                Command = this,
                Id = Guid.NewGuid(),
                Range = new TimeRange
                {
                    Start = startTime,
                    End = startTime.AddDays(1)
                }
            };
        }

        #endregion

        /// <summary>
        /// Maps a command to the database.
        /// </summary>
        public class CommandMapping : ClassMap<Command>
        {
            /// <summary>
            /// Maps a command to the database.
            /// </summary>
            public CommandMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Name).Not.Nullable().Unique();
                Map(x => x.Description);
                Map(x => x.Address).Not.Nullable();
                Map(x => x.City).Not.Nullable();
                Map(x => x.State).Not.Nullable();
                Map(x => x.ZipCode).Not.Nullable();
                Map(x => x.Country).Not.Nullable();
                Map(x => x.MusterStartHour).Not.Nullable();
                Map(x => x.TimeZoneId).Not.Nullable();

                HasMany(x => x.Departments).Cascade.All();

                References(x => x.CurrentMusterCycle).Cascade.All().Not.Nullable();

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the Command.
        /// </summary>
        public class Validator : AbstractValidator<Command>
        {
            /// <summary>
            /// Validates the Command.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Description).Length(0, 255);
                RuleFor(x => x.Name).NotEmpty().Length(1, 20);
                RuleFor(x => x.Address).NotEmpty().Length(1, 255);
                RuleFor(x => x.City).NotEmpty().Length(1, 255);
                RuleFor(x => x.State).NotEmpty().Length(1, 255);
                RuleFor(x => x.ZipCode).NotEmpty().Length(1, 255);
                RuleFor(x => x.Country).NotEmpty().Length(1, 255);

                RuleFor(x => x.MusterStartHour).InclusiveBetween(1, 24);

                RuleFor(x => x.CurrentMusterCycle).NotEmpty();
                RuleFor(x => x.TimeZoneId).Must(x =>
                {
                    try
                    {
                        TimeZoneInfo.FindSystemTimeZoneById(x);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
        }
    }
}
