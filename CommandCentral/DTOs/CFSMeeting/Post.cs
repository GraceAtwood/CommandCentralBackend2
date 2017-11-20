using System;
using CommandCentral.Utilities.Types;

namespace CommandCentral.DTOs.CFSMeeting
{
    public class Post : Put
    {
        public Guid Request { get; set; }
        public Guid Advisor { get; set; }
        public Guid Person { get; set; }
        public TimeRange Range { get; set; }
    }
}