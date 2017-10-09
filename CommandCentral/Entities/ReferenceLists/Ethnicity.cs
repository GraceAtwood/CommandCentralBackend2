using FluentNHibernate.Mapping;
using FluentValidation;
using CommandCentral.Framework;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single ethnicity
    /// </summary>
    public class Ethnicity : ReferenceListItemBase
    {
        /// <summary>
        /// Maps an ethnicity to the database.
        /// </summary>
        public class EthnicityMapping : SubclassMap<Ethnicity>
        {
        }
    }
}