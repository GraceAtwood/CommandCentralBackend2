using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.Division
{
    public class Put : Post
    {
        public Guid Department { get; set; }
    }
}
