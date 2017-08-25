using System;

namespace CommandCentral.DTOs.PasswordReset
{
    /// <summary>
    /// The DTO the client sends us to complete a password reset.
    /// </summary>
    public class PostComplete
    {
        /// <summary>
        /// The new password for this account.
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// The ResetToken we provided them in the url in their email.
        /// </summary>
        public Guid ResetToken { get; set; }
    }
}