using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Identifies different types of correspondence.
    /// </summary>
    public class CorrespondenceItemType : ReferenceListItemBase
    {
        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class CorrespondenceItemTypeMapping : SubclassMap<CorrespondenceItemType>
        {
        }
    }
}