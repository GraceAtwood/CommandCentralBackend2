using System;

namespace CommandCentral.DTOs.Building
{
    public class Get : Post
    {
        public Guid Id { get; set; }

        public Get(Entities.BEQ.Building building)
        {
            Id = building.Id;
            Command = building.Command.Id;
            Description = building.Description;
            Name = building.Name;
        }
    }
}