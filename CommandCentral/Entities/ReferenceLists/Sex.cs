using System;
using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Indicates the sex of a given person.
    /// </summary>
    public class Sex : ReferenceListItemBase
    {
        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Maps the object to the database.
        /// </summary>
        public class SexMapping : SubclassMap<Sex>
        {
        }
    }
}