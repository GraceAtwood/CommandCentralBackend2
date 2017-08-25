using System;
using FluentNHibernate.Mapping;
using FluentValidation;
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
        /// <returns>Returns true if registration is expired, false otherwise.</returns>
        public virtual bool IsAgedOff()
        {
            return DateTime.UtcNow.Subtract(TimeSubmitted) > _maxAge;
        }

        #endregion

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Maps this class to the database.
        /// </summary>
        public class AccountRegistrationMapping : ClassMap<AccountRegistration>
        {
            /// <summary>
            /// Maps this class to the database.
            /// </summary>
            public AccountRegistrationMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.TimeSubmitted).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.IsCompleted).Not.Nullable();
                Map(x => x.TimeCompleted).CustomType<UtcDateTimeType>();
                Map(x => x.RegistrationToken).Not.Nullable().Unique();

                References(x => x.Person).Not.Nullable().Unique();
            }
        }

        /// <summary>
        /// Validator for AccountRegistration
        /// </summary>
        public class Validator : AbstractValidator<AccountRegistration>
        {
            /// <summary>
            /// Validate this AccountRegistration entity
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Person).NotEmpty();
                RuleFor(x => x.RegistrationToken).NotEmpty();
                RuleFor(x => x.TimeSubmitted).NotEmpty();
                When(x => x.IsCompleted || x.TimeCompleted.HasValue, () =>
                {
                    RuleFor(x => x.IsCompleted).Equal(true);
                    RuleFor(x => x.TimeCompleted).NotEmpty();
                });
            }
        }
    }
}
