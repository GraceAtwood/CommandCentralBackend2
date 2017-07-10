using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Indicates the sex of a given person.
    /// </summary>
    public class Sex : ReferenceListItemBase
    {
        /// <summary>
        /// Maps the object to the database.
        /// </summary>
        public class SexMapping : ClassMap<Sex>
        {
            /// <summary>
            /// Maps the object to the database.
            /// </summary>
            public SexMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }
    }
}
