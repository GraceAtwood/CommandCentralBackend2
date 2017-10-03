using System;

namespace CommandCentral.DTOs.CollateralDutyMembership
{
    public class Post : Put
    {
        public Guid Person { get; set; }
        public Guid CollateralDuty { get; set; }
    }
}