using System;

namespace CommandCentral.DTOs.Registration
{
    public class PostComplete
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public Guid RegistrationToken { get; set; }
    }
}