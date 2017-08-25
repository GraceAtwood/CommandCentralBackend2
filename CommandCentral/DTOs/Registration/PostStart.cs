namespace CommandCentral.DTOs.Registration
{
    /// <summary>
    /// The DTO for starting a registration request.
    /// </summary>
    public class PostStart
    {
        /// <summary>
        /// The SSN for the user wanting to register.
        /// </summary>
        public string SSN { get; set; }
        
        /// <summary>
        /// The stub of the frontend link the user should go to in order to finish registration. We will add a token to
        /// this and put it in the email.
        /// </summary>
        public string ContinueLink { get; set; }
    }
}