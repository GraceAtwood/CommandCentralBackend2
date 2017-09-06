using CommandCentral.Entities;
using CommandCentral.Entities.Correspondence;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model used on the corr routed emails.
    /// </summary>
    public class CorrespondenceRouted
    {
        /// <summary>
        /// The person to whom the email is being sent.
        /// </summary>
        public Person To { get; }
        
        /// <summary>
        /// The correspondence item referenced in this email.
        /// </summary>
        public CorrespondenceItem CorrespondenceItem { get; }

        /// <summary>
        /// The new person the referenced corr item was routed to.
        /// </summary>
        public Person NewPersonRoutedTo { get; }

        /// <summary>
        /// Creates a new email model.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="item"></param>
        /// <param name="newPersonRoutedTo"></param>
        public CorrespondenceRouted(Person to, CorrespondenceItem item, Person newPersonRoutedTo)
        {
            To = to;
            CorrespondenceItem = item;
            NewPersonRoutedTo = newPersonRoutedTo;
        }
    }
}