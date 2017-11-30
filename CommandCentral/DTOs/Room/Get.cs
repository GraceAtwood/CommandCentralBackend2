using System;

namespace CommandCentral.DTOs.Room
{
    public class Get : Post
    {
        public Guid Id { get; set; }

        public Get(Entities.BEQ.Room room)
        {
            Id = room.Id;
            Building = room.Building.Id;
            Level = room.Level;
            Number = room.Number;
            PersonAssigned = room.PersonAssigned?.Id;
        }
    }
}