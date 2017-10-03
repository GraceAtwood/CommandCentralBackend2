using System;

namespace CommandCentral.DTOs.CollateralDuty
{
    public class Get : Update
    {
        public Guid Id { get; set; }
        public Guid Command { get; set; }

        public Get(Entities.CollateralDutyTracking.CollateralDuty item)
        {
            Id = item.Id;
            Command = item.Command.Id;
            Name = item.Name;
        }
    }
}