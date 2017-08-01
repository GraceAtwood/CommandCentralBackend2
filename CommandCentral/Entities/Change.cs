using System;
using FluentNHibernate.Mapping;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using NHibernate.Type;
using CommandCentral.Utilities;
using FluentValidation.Results;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single change
    /// </summary>
    public class Change : Entity
    {
        #region Properties

        /// <summary>
        /// The client who initiated this change.
        /// </summary>
        public virtual Person Editor { get; set; }

        /// <summary>
        /// The person who was edited.
        /// </summary>
        public virtual Person Editee { get; set; }

        /// <summary>
        /// The name of the property of the object that changed.
        /// </summary>
        public virtual string PropertyName { get; set; }

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
        public virtual DateTime Time { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Casts the
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"The property '{PropertyName}' changed from '{OldValue}' to '{NewValue}'.";
        }

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
                References(x => x.Editee).Not.Nullable();

                Map(x => x.Time).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.PropertyName).Not.Nullable();
                Map(x => x.OldValue);
                Map(x => x.NewValue);
            }
        }

    }
}
