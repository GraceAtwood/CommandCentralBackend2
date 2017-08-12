using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.CorrespondenceItem
{
    public class Post : Put
    {
        public Guid SubmittedFor { get; set; }
        public Guid Type { get; set; }
    }
}
