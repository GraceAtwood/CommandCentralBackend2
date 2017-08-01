using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.Division
{
    public class Update
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid Department { get; set; }
    }
}
