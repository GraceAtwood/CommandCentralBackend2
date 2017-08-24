using System;

namespace CommandCentral.DTOs.CorrespondenceReview
{
    public class Post
    {
        public Guid Reviewer { get; set; }
        public bool IsFinal { get; set; }
    }
}
