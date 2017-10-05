using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using CommandCentral.Framework;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single UIC.
    /// </summary>
    public class UIC : ReferenceListItemBase
    {
        /// <summary>
        /// Maps a UIC to the database.
        /// </summary>
        public class UICMapping : SubclassMap<UIC>
        {
        }
    }
}
