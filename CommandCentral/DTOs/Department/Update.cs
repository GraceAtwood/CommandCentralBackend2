using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.Department
{
    public class Update
    {
        public Guid Command { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
