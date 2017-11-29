using CommandCentral.Enums;

namespace CommandCentral.DTOs.CollateralDutyMembership
{
    public class Put
    {
        public CollateralRoles Role { get; set; }
        public ChainOfCommandLevels Level { get; set; }
    }
}