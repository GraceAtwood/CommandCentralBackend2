using System;

namespace CommandCentral.DTOs.CollateralDutyMembership
{
    public class Get : Post
    {
        public Guid Id { get; set; }

        public Get(Entities.CollateralDutyTracking.CollateralDutyMembership item)
        {
            Id = item.Id;
            CollateralDuty = item.CollateralDuty.Id;
            Role = item.Role;
            Level = item.Level;
            Person = item.Person.Id;
        }
    }
}