using CommandCentral.Entities;
using CommandCentral.Entities.CollateralDutyTracking;

namespace CommandCentral.Email.Models
{
    public class CollateralMembershipCreated
    {
        public Person To { get; }
        public CollateralDutyMembership CollateralDutyMembership { get; }

        public CollateralMembershipCreated(Person to, CollateralDutyMembership collateralDutyMembership)
        {
            To = to;
            CollateralDutyMembership = collateralDutyMembership;
        }
        
    }
}