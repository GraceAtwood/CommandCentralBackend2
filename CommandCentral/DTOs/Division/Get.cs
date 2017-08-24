using System;

namespace CommandCentral.DTOs.Division
{
    public class Get : Put
    {
        public Guid Id { get; set; }

        public Get(Entities.Division division)
        {
            Department = division.Department.Id;
            Description = division.Description;
            Id = division.Id;
            Name = division.Name;
        }
    }
}
