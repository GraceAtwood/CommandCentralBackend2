using System;

namespace CommandCentral.DTOs.CorrespondenceItem
{
    public class Post : Put
    {
        public Guid SubmittedFor { get; set; }
        public Guid Type { get; set; }
    }
}
