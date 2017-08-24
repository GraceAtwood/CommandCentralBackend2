using System;

namespace CommandCentral.DTOs.Registration
{
    /// <summary>
    /// The DTO the client sends us to complete registration
    /// </summary>
    public class PostComplete
    {
        /// <summary>
        /// The new username
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// The new password
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// The RegistrationToken we provided them in the url in their email
        /// </summary>
        public Guid RegistrationToken { get; set; }
    }
}