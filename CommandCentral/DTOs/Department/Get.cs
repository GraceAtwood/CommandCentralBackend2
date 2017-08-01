using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.Department
{
    public class Get : Update
    {
        public Guid Id { get; set; }
        public List<Guid> Divisions { get; set; }
    }
}
