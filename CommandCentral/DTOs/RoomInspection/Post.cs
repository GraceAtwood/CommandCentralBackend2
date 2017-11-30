using System;

namespace CommandCentral.DTOs.RoomInspection
{
    public class Post : Put
    {
        public Guid[] InspectedBy { get; set; }
        public Guid Person { get; set; }
        public Guid Room { get; set; }
        public DateTime Time { get; set; }
    }
}