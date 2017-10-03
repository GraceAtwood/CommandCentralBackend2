using CommandCentral.Enums;

namespace CommandCentral.DTOs.CollateralDutyMembership
{
    public class Put
    {
        public CollateralRoles Role { get; set; }
        public CollateralLevels Level { get; set; }
    }
}