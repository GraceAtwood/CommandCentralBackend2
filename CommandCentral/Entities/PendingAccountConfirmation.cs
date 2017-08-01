using System;
using FluentNHibernate.Mapping;
using FluentValidation.Results;
using NHibernate.Type;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single account confirmation.  This is created when a client attempts to register an account.
    /// </summary>
    public class PendingAccountConfirmation : Entity
    {

        /// <summary>
        /// The max age after which an account confirmation will have expired and it will become invalid.
        /// </summary>
        private static readonly TimeSpan _maxAge = TimeSpan.FromDays(1);

        #region Properties

        /// <summary>
        /// The person to which it belongs.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// The time at which this was created.
        /// </summary>
        public virtual DateTime Time { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// returns a boolean indicating whether or not this account confirmation is still valid or if it has aged off.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid()
        {
            return DateTime.UtcNow.Subtract(Time) < _maxAge;
        }

        #endregion

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Maps this class to the database.
        /// </summary>
        public class PendingAccountConfirmationMapping : ClassMap<PendingAccountConfirmation>
        {
            /// <summary>
            /// Maps this class to the database.
            /// </summary>
            public PendingAccountConfirmationMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Person).Not.Nullable().Unique();

                Map(x => x.Time).Not.Nullable().CustomType<UtcDateTimeType>();
            }
        }

    }
}
