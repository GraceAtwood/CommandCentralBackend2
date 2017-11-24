using System;

namespace CommandCentral.DTOs.PhysicalAddress
{
    public class Post : Put
    {
        public Guid Person { get; set; }
    }
}