using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.CorrespondenceReview
{
    public class Post
    {
        public Guid Reviewer { get; set; }
        public bool IsFinal { get; set; }
    }
}
