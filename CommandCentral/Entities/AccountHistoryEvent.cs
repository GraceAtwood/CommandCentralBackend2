using System;
using CommandCentral.Authorization;
using FluentNHibernate.Mapping;
using NHibernate.Type;
using FluentValidation.Results;
using CommandCentral.Enums;
using CommandCentral.Framework;
using FluentValidation;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single account history event.
    /// </summary>
    public class AccountHistoryEvent : Entity
    {
        #region Properties
        
        /// <summary>
        /// The type of history event that occurred.
        /// </summary>
        public virtual AccountHistoryTypes AccountHistoryEventType { get; set; }

        /// <summary>
        /// The time at which this event occurred.
        /// </summary>
        public virtual DateTime EventTime { get; set; }

        /// <summary>
        /// The person whose profile this account history event is on.
        /// </summary>
        public virtual Person Person { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the EventType @ Time
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{AccountHistoryEventType} @ {EventTime}";
        }

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
        /// Maps an account history event to the database.
        /// </summary>
        public class AccountHistoryEventMapping : ClassMap<AccountHistoryEvent>
        {
            /// <summary>
            /// Maps an account history event to the database.
            /// </summary>
            public AccountHistoryEventMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.EventTime).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.AccountHistoryEventType).Not.Nullable();

                References(x => x.Person).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<AccountHistoryEvent>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.EventTime).NotEmpty();
                RuleFor(x => x.Person).NotEmpty();
            }
        }

        /// <summary>
        /// Rules for this object.
        /// </summary>
        public class Contract : RulesContract<AccountHistoryEvent>
        {
            /// <summary>
            /// Rules for this ojbect.
            /// </summary>
            public Contract()
            {
                RulesFor()
                    .CanEdit((person, @event) => false)
                    .CanReturn((person, @event) => true);
            }
        }
    }
}
