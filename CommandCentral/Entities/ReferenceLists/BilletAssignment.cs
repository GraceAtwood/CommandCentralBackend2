using System;
using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a billet assignment: P2 or P3 which is how the Navy knows who undersigns a person's billet payment.
    /// </summary>
    public class BilletAssignment : ReferenceListItemBase
    {
        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class BilletAssignmentMapping : SubclassMap<BilletAssignment>
        {
        }
    }
}