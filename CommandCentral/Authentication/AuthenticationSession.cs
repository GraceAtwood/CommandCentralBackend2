using System;
using FluentNHibernate.Mapping;
using CommandCentral.Entities;
using FluentValidation.Results;

namespace CommandCentral.Authentication
{
    /// <summary>
    /// Describes a single authentication session and provides members for interacting with that session.
    /// </summary>
    public class AuthenticationSession : Entity
    {
        /// <summary>
        /// The max age after which a session will have expired and it will become invalid.
        /// </summary>
        private static readonly TimeSpan _maxAge = TimeSpan.FromMinutes(20);

        #region Properties

        /// <summary>
        /// This is the actual token that should be served to the client.
        /// </summary>
        public virtual Guid Token { get; set; }

        /// <summary>
        /// The time this session was created which is the time the client logged in.
        /// </summary>
        public virtual DateTime LoginTime { get; set; }

        /// <summary>
        /// The person to whom this session belongs.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// The time at which the client logged out, thus invalidating this session.
        /// </summary>
        public virtual DateTime LogoutTime { get; set; }

        /// <summary>
        /// Indicates where or not the session is valid.
        /// </summary>
        public virtual bool IsActive { get; set; }

        /// <summary>
        /// The last time this session was used, not counting this current time.
        /// </summary>
        public virtual DateTime LastUsedTime { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines if this session has expired given a max age of inactivity.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid()
        {
            if (!IsActive)
                return false;

            #if DEBUG
            return true;
            #endif

            return DateTime.UtcNow.Subtract(LastUsedTime) < _maxAge;
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Maps a session to the database.
        /// </summary>
        public class SessionMapping : ClassMap<AuthenticationSession>
        {
            /// <summary>
            /// Maps a session to the database.
            /// </summary>
            public SessionMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.LoginTime).Not.Nullable();
                Map(x => x.LogoutTime).Nullable();
                Map(x => x.IsActive).Not.Nullable();
                Map(x => x.LastUsedTime);
                Map(x => x.Token).Not.Nullable().Unique();
                
                References(x => x.Person).LazyLoad(Laziness.False);
            }
        }
    }
}
