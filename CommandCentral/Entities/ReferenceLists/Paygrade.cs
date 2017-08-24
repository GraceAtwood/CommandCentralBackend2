using System;
using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CommandCentral.Entities.ReferenceLists
{
    public class Paygrade : ReferenceListItemBase
    {
        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public class PaygradeMapping : ClassMap<Paygrade>
        {
            public PaygradeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }
    }
}
