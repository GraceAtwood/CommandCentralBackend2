using System;
using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// A watch qualification, such as JOOD, OOD, etc.
    /// </summary>
    public class WatchQualification : ReferenceListItemBase
    {
        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchQualificationMapping : SubclassMap<WatchQualification>
        {
        }
    }
}