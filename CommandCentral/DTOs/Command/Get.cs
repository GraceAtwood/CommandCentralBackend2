using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandCentral.DTOs.Command
{
    public class Get : Update
    {
        public Guid Id { get; set; }
        public List<Guid> Departments { get; set; }

        public Get(Entities.Command item)
        {
            Id = item.Id;
            Name = item.Name;
            Description = item.Description;
            Departments = item.Departments.Select(x => x.Id).ToList();
        }
    }
}
