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

        /// <summary>
        /// Indicates that this account confirmation has been completed.
        /// </summary>
        public virtual bool IsCompleted { get; set; }

        /// <summary>
        /// The time that this account registration was completed.
        /// </summary>
        public virtual DateTime? TimeCompleted { get; set; }

        /// <summary>
        /// The token that was sent to the client via email.
        /// </summary>
        public virtual Guid ConfirmationToken { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Returns a boolean indicating whether or not this account confirmation is still valid or if it has aged off.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAgedOff()
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

                Map(x => x.Time).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.IsCompleted).Not.Nullable();
                Map(x => x.TimeCompleted).CustomType<UtcDateTimeType>();
                Map(x => x.ConfirmationToken).Not.Nullable();

                References(x => x.Person).Not.Nullable().Unique();
            }
        }
    }
}
