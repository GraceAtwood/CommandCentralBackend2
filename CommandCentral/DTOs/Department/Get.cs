using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.Department
{
    public class Get : Update
    {
        public Guid Id { get; set; }

        public Get(Entities.Department department)
        {
            Id = department.Id;
            Command = department.Command.Id;
            Description = department.Description;
            Name = department.Name;
        }
    }
}
