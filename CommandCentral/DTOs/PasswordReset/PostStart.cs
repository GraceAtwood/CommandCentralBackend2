namespace CommandCentral.DTOs.PasswordReset
{
    /// <summary>
    /// The DTO for starting a password reset.
    /// </summary>
    public class PostStart
    {
        /// <summary>
        /// The requester's military email.
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// The requester's SSN.
        /// </summary>
        public string SSN { get; set; }
        
        /// <summary>
        /// The stub of the frontend link the user should go to in order to finish password reset. We will add a token
        /// to this and put it in the email.
        /// </summary>
        public string ContinueLink { get; set; }
    }
}