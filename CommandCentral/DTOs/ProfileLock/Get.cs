using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.ProfileLock
{
    public class Get : Update
    {
        public Guid Id { get; set; }
        public Guid Owner { get; set; }
        public DateTime SubmitTime { get; set; }
        public TimeSpan MaxAge { get; set; }
    }
}
