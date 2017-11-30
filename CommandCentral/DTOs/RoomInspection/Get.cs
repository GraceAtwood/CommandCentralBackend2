using System;

namespace CommandCentral.DTOs.RoomInspection
{
    public class Get : Post
    {
        public Guid Id { get; set; }
    }
}