using CommandCentral.Entities;

namespace CommandCentral.Email.Models
{
    public class AccountRegistered
    {
        public Person To { get; }
        public AccountRegistration AccountRegistration { get; }

        public AccountRegistered(Person to, AccountRegistration accountRegistration)
        {
            To = to;
            AccountRegistration = accountRegistration;
        }
        
    }
}