using System;

namespace CommandCentral.DTOs.Department
{
    public class Get : Put
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
