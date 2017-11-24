using System;

namespace CommandCentral.DTOs.EmailAddress
{
    public class Post : Put
    {
        public Guid Person { get; set; }
    }
}