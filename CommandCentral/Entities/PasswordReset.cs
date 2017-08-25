using System;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using NHibernate.Type;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes the password reset process, specifically between initiation and completion.
    /// </summary>
    public class PasswordReset : Entity
    {
        /// <summary>
        /// The max age after which password reset will have expired and become invalid.
        /// </summary>
        private static readonly TimeSpan _maxAge = TimeSpan.FromDays(1);
        
        #region Properties
        
        /// <summary>
        /// The person who's password we're resetting
        /// </summary>
        public virtual Person Person { get; set; }
        
        /// <summary>
        /// The time this password reset was initiated
        /// </summary>
        public virtual DateTime TimeSubmitted { get; set; }
        
        /// <summary>
        /// The token we send to their email to perform the reset
        /// </summary>
        public virtual Guid ResetToken { get; set; }
        
        #endregion
        
        #region Helper Methods

        /// <summary>
        /// Returns a boolean indicating if this reset has expired.
        /// </summary>
        /// <returns>Returns true if this reset is expired, false otherwise.</returns>
        public virtual bool IsAgedOff()
        {
            return DateTime.UtcNow.Subtract(TimeSubmitted) > _maxAge;
        }
        
        #endregion
        
        /// <summary>
        /// Returns whether this is valid.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// The Validator for PasswordReset
        /// </summary>
        public class Validator : AbstractValidator<PasswordReset>
        {
            /// <summary>
            /// Create a validator for PasswordReset
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Person).NotEmpty();
                RuleFor(x => x.ResetToken).NotEmpty();
                RuleFor(x => x.TimeSubmitted).NotEmpty();
            }
        }

        /// <summary>
        /// Map this class to the database.
        /// </summary>
        public class PasswordResetMapping : ClassMap<PasswordReset>
        {
            /// <summary>
            /// Map this class to the database.
            /// </summary>
            public PasswordResetMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.TimeSubmitted).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.ResetToken).Not.Nullable().Unique();

                References(x => x.Person).Not.Nullable().Unique();
            }
        }
    }
}