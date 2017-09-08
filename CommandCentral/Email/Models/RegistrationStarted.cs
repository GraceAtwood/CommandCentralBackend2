using CommandCentral.Entities;

namespace CommandCentral.Email.Models
{
    public class RegistrationStarted
    {
        public Person Person { get; }
        public string CountinueLink { get; }

        public RegistrationStarted(Person person, string countinueLink)
        {
            Person = person;
            CountinueLink = countinueLink;
        }
        
    }
}