using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    public class MusterStatus : ReferenceListItemBase
    {
        public class MusterStatusMapping : ClassMap<MusterStatus>
        {
            public MusterStatusMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }
    }
}
