using System;

namespace CommandCentral.DTOs.Comment
{
    public class Post : Put
    {
        public Guid OwningEntity { get; set; }
    }
}
