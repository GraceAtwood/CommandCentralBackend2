using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.Command
{
    public class Get : Update
    {
        public Guid Id { get; set; }
        public List<Guid> Departments { get; set; }
    }
}
