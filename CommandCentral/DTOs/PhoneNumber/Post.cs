using System;

namespace CommandCentral.DTOs.PhoneNumber
{
    public class Post : Put
    {
        public Guid Person { get; set; }
    }
}