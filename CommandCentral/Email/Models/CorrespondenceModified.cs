using CommandCentral.Entities;
using CommandCentral.Entities.Correspondence;

namespace CommandCentral.Email.Models
{
    public class CorrespondenceModified
    {
        public Person To { get; }
        public CorrespondenceItem CorrespondenceItem { get; }

        internal CorrespondenceModified(Person to, CorrespondenceItem item)
        {
            To = to;
            CorrespondenceItem = item;
        }
    }
}