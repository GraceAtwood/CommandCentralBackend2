using System;

namespace CommandCentral.DTOs.WorkOrder
{
    public class Update
    {
        public string Body { get; set; }
        public string Location { get; set; }
        public Guid? RoomLocation { get; set; }
    }
}