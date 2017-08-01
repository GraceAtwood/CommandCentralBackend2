using System;
using FluentNHibernate.Mapping;
using CommandCentral.Entities.ReferenceLists;
using NHibernate.Type;
using CommandCentral.Utilities;
using FluentValidation.Results;

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
        public virtual AccountHistoryType AccountHistoryEventType { get; set; }

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
        /// Not implemented
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
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

                References(x => x.AccountHistoryEventType).Not.Nullable();
                References(x => x.Person).Not.Nullable();
                
            }
        }
    }
}
