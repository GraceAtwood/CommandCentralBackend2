using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using CommandCentral.Framework;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single designation.  This is the job title for civilians, the rate for enlisted and the designator for officers.
    /// </summary>
    public class Designation : ReferenceListItemBase
    {
        /// <summary>
        /// Maps a Designation to the database.
        /// </summary>
        public class DesignationMapping : SubclassMap<Designation>
        {
        }
    }
}