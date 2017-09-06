using CommandCentral.Entities;
using CommandCentral.Entities.Correspondence;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The generic email model used on most correspondence emails.
    /// </summary>
    public class CorrespondenceGeneric
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
        /// Creates a new email model.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="item"></param>
        public CorrespondenceGeneric(Person to, CorrespondenceItem item)
        {
            To = to;
            CorrespondenceItem = item;
        }
    }
}