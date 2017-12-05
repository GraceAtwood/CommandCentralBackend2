using System;

namespace CommandCentral.DTOs.WatchShiftType
{
    public class Get : Update
    {
        public Guid Id { get; set; }

        public Get(Entities.Watchbill.WatchShiftType item)
        {
            Id = item.Id;
            Name = item.Name;
            Description = item.Description;
            Qualification = item.Qualification;
        }

        public Get()
        {
        }
    }
}