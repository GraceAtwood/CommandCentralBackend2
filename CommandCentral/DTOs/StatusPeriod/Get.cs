using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.StatusPeriod
{
    public class Get : Post
    {
        public Guid Id { get; set; }
        public DateTime DateSubmitted { get; set; }
        public Guid SubmittedBy { get; set; }
        public Guid LastModifiedBy { get; set; }
        public DateTime DateLastModified { get; set; }
    }
}
