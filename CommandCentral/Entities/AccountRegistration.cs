using System;
using FluentNHibernate.Mapping;
using FluentValidation.Results;
using NHibernate.Type;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single account registration.  This is created when a client attempts to register an account.
    /// </summary>
    public class AccountRegistration : Entity
    {
        /// <summary>
        /// The max age after which an account registration will have expired and it will become invalid.
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
        public virtual DateTime TimeSubmitted { get; set; }

        /// <summary>
        /// Indicates that this account registration has been completed.
        /// </summary>
        public virtual bool IsCompleted { get; set; }

        /// <summary>
        /// The time that this account registration was completed.
        /// </summary>
        public virtual DateTime? TimeCompleted { get; set; }

        /// <summary>
        /// The token that was sent to the client via email.
        /// </summary>
        public virtual Guid RegistrationToken { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Returns a boolean indicating whether or not this account registration is still valid or if it has aged off.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAgedOff()
        {
            return DateTime.UtcNow.Subtract(TimeSubmitted) < _maxAge;
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
        public class PendingAccountConfirmationMapping : ClassMap<AccountRegistration>
        {
            /// <summary>
            /// Maps this class to the database.
            /// </summary>
            public PendingAccountConfirmationMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.TimeSubmitted).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.IsCompleted).Not.Nullable();
                Map(x => x.TimeCompleted).CustomType<UtcDateTimeType>();
                Map(x => x.RegistrationToken).Not.Nullable().Unique();

                References(x => x.Person).Not.Nullable().Unique();
            }
        }
    }
}
