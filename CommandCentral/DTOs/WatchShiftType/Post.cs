using System;
using CommandCentral.Enums;

namespace CommandCentral.DTOs.WatchShiftType
{
    public class Post : Put
    {
        public Guid Command { get; set; }
    }
}