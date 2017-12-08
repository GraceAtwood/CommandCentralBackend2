using System;

namespace CommandCentral.DTOs.WatchShiftType
{
    public class Get : Post
    {
        public Guid Id { get; set; }

        public Get(Entities.Watchbill.WatchShiftType item)
        {
            Id = item.Id;
            Name = item.Name;
            Description = item.Description;
            Qualification = item.Qualification;
            Command = item.Command.Id;
        }

        public Get()
        {
        }
    }
}