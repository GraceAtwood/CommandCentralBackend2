using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Provides abstracted access to a reference list such as Ranks or Rates.
    /// </summary>
    public abstract class ReferenceListItemBase : Entity
    {
        #region Properties
        
        /// <summary>
        /// The value of this item.
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// A description of this item.
        /// </summary>
        public virtual string Description { get; set; }

        #endregion

        /// <summary>
        /// Maps all reference lists to the database.
        /// </summary>
        public class ReferenceListItemBaseMap : ClassMap<ReferenceListItemBase>
        {
            /// <summary>
            /// Maps all reference lists to the database.
            /// </summary>
            public ReferenceListItemBaseMap()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
                
                UseUnionSubclassForInheritanceMapping();
            }
        }
    }
}
