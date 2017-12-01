using System;
using System.Linq;

namespace CommandCentral.DTOs.RoomInspection
{
    public class Get : Post
    {
        public Guid Id { get; set; }

        public Get(Entities.BEQ.RoomInspection item)
        {
            Id = item.Id;
            InspectedBy = item.InspectedBy.Select(x => x.Id).ToArray();
            Person = item.Person.Id;
            Room = item.Room.Id;
            Score = item.Score;
            Time = item.Time;
        }
    }
}