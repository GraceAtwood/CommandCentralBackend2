using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Identifies the type of a cfs request.
    /// </summary>
    public class CFSRequestType : ReferenceListItemBase
    {
        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class CFSRequestTypeMapping : SubclassMap<CFSRequestType>
        {
        }
    }
}