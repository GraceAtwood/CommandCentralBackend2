using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.Department
{
    public class Put : Post
    {
        public Guid Command { get; set; }
    }
}
