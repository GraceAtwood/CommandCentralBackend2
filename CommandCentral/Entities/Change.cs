using System;
using FluentNHibernate.Mapping;
using NHibernate.Type;
using FluentValidation.Results;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single change.
    /// </summary>
    public class Change : Entity
    {
        #region Properties

        /// <summary>
        /// The client who initiated this change.
        /// </summary>
        public virtual Person Editor { get; set; }

        /// <summary>
        /// The entity on which the property was modified.
        /// </summary>
        public virtual Entity Entity { get; set; }

        /// <summary>
        /// The path to the property that was modified.
        /// </summary>
        public virtual string PropertyPath { get; set; }

        /// <summary>
        /// The value prior to the update or change.
        /// </summary>
        public virtual string OldValue { get; set; }

        /// <summary>
        /// The new value.
        /// </summary>
        public virtual string NewValue { get; set; }

        /// <summary>
        /// The time this change was made.
        /// </summary>
        public virtual DateTime ChangeTime { get; set; }

        #endregion

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Maps a change to the database.
        /// </summary>
        public class ChangeMapping : ClassMap<Change>
        {
            /// <summary>
            /// Maps a change to the database.
            /// </summary>
            public ChangeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Editor).Not.Nullable();

                Map(x => x.ChangeTime).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.PropertyPath).Not.Nullable();
                Map(x => x.OldValue);
                Map(x => x.NewValue);
                
                ReferencesAny(x => x.Entity)
                    .AddMetaValue<NewsItem>(typeof(NewsItem).Name)
                    .IdentityType<Guid>()
                    .EntityTypeColumn("Entity_Type")
                    .EntityIdentifierColumn("Entity_id")
                    .MetaType<string>();
            }
        }

    }
}
