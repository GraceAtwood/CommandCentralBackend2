using System;
using CommandCentral.Utilities.Types;

namespace CommandCentral.DTOs.CFSMeeting
{
    public class Post : Put
    {
        public Guid Person { get; set; }
        public Guid Advisor { get; set; }
        public Guid Request { get; set; }
        public TimeRange
    }
}