using System;

namespace CommandCentral.DTOs.Building
{
    public class Post : Put
    {
        public Guid Command { get; set; }
    }
}