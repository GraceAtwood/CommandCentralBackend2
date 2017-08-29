using CommandCentral.Entities;

namespace CommandCentral.Email.Models
{
    public class RegistrationStarted
    {
        public Person Person { get; }
        public string CountinueLink { get; }

        internal RegistrationStarted(Person person, string countinueLink)
        {
            Person = person;
            CountinueLink = countinueLink;
        }
        
    }
}