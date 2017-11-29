using System;
using CommandCentral.Authorization;
using CommandCentral.Enums;
using CommandCentral.Framework;
using FluentNHibernate.Mapping;
using NHibernate.Type;
using FluentValidation.Results;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single profile lock.
    /// </summary>
    public class ProfileLock : Entity
    {
        /// <summary>
        /// The maximum age, as a timespan, after which a profile lock should be considered invalid.
        /// </summary>
        public static TimeSpan MaxAge { get; } = TimeSpan.FromMinutes(20);

        #region Properties

        /// <summary>
        /// The person who owns this lock.
        /// </summary>
        public virtual Person Owner { get; set; }

        /// <summary>
        /// The Person whose profile is locked.
        /// </summary>
        public virtual Person LockedPerson { get; set; }

        /// <summary>
        /// The time at which this lock was submitted.
        /// </summary>
        public virtual DateTime SubmitTime { get; set; }

        #endregion

        #region Helper Methods
        
        /// <summary>
        /// Returns a booleans indicating if the current ProfileLock is valid - compares against the max age found in the config.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsExpired()
        {
            return DateTime.UtcNow.Subtract(SubmitTime) >= MaxAge;
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
        /// Maps a profile lock to the database.
        /// </summary>
        public class ProfileLockMapping : ClassMap<ProfileLock>
        {
            /// <summary>
            /// Maps a profile lock to the database.
            /// </summary>
            public ProfileLockMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Owner).Not.Nullable().Unique();
                References(x => x.LockedPerson).Not.Nullable().Unique();

                Map(x => x.SubmitTime).Not.Nullable().CustomType<UtcDateTimeType>();
            }
        }
        
        public class Contract : RulesContract<ProfileLock>
        {
            public Contract()
            {
                RulesFor()
                    .CanEdit((person, profileLock) =>
                    {
                        if (person.SpecialPermissions.Contains(SpecialPermissions.AdminTools))
                            return true;

                        if (profileLock.Owner == person)
                            return true;

                        if (profileLock.IsExpired())
                            return true;

                        return false;
                    })
                    .CanReturn((person, profileLock) => true);
            }
        }
    }
}
