﻿using System;
using FluentNHibernate.Mapping;
using FluentValidation.Results;
using NHibernate.Type;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Defines a single attempt to reset a password.  The Id of this class is the key that unlocks the Finish Password Reset endpoint.
    /// </summary>
    public class PendingPasswordReset : Entity
    {

        /// <summary>
        /// The max age after which a password reset will have expired and it will become invalid.
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
        /// returns a boolean indicating whether or not this password reset is still valid or if it has aged off.
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
        public class PendingPasswordResetMapping : ClassMap<PendingPasswordReset>
        {
            /// <summary>
            /// Maps this class to the database.
            /// </summary>
            public PendingPasswordResetMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Person).Not.Nullable().Unique();

                Map(x => x.Time).Not.Nullable().CustomType<UtcDateTimeType>();
            }
        }
    }
}