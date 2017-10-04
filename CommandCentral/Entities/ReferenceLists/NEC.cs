using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using CommandCentral.Framework;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single NEC.
    /// </summary>
    public class NEC : ReferenceListItemBase
    {
        /// <summary>
        /// Maps an NEC to the database.
        /// </summary>
        public class NECMapping : SubclassMap<NEC>
        {
        }
    }
}