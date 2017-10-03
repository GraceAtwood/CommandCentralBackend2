using CommandCentral.Entities;
using CommandCentral.Entities.CollateralDutyTracking;

namespace CommandCentral.Email.Models
{
    public class CollateralDeleted
    {
        public Person To { get; }
        public CollateralDuty CollateralDuty { get; }

        public CollateralDeleted(Person to, CollateralDuty collateralDuty)
        {
            To = to;
            CollateralDuty = collateralDuty;
        }
        
    }
}