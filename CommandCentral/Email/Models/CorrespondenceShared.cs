using System.Collections.Generic;
using CommandCentral.Entities;
using CommandCentral.Entities.Correspondence;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model used on correspondence emails to indicate a corr has just been shared with new persons.
    /// </summary>
    public class CorrespondenceShared
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
        /// The list of new persons this corr item was just shared with.
        /// </summary>
        public List<Person> Added { get; }
        
        /// <summary>
        /// The list of persons that were removed from the corr item.
        /// </summary>
        public List<Person> Removed { get; }

        /// <summary>
        /// Creates a new email model.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="item"></param>
        /// <param name="added"></param>
        /// <param name="removed"></param>
        public CorrespondenceShared(Person to, CorrespondenceItem item, IEnumerable<Person> added,
            IEnumerable<Person> removed)
        {
            To = to;
            CorrespondenceItem = item;
            Added = new List<Person>(added);
            Removed = new List<Person>(removed);
        }
    }
}