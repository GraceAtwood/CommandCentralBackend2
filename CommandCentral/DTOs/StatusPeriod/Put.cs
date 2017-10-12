using CommandCentral.Utilities.Types;
using CommandCentral.Enums;

namespace CommandCentral.DTOs.StatusPeriod
{
    public class Put
    {
        public AccountabilityTypes Reason { get; set; }
        public TimeRange Range { get; set; }
        public bool ExemptsFromWatch { get; set; }
    }
}
