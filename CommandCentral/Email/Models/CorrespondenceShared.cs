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
        public List<Person> NewPersons { get; }

        /// <summary>
        /// Creates a new email model.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="item"></param>
        /// <param name="newPersons"></param>
        public CorrespondenceShared(Person to, CorrespondenceItem item, IEnumerable<Person> newPersons)
        {
            To = to;
            CorrespondenceItem = item;
            NewPersons = new List<Person>(newPersons);
        }
    }
}