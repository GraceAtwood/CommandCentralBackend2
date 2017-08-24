using System;
using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CommandCentral.Entities.ReferenceLists
{
    public class PhoneNumberType : ReferenceListItemBase
    {
        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public class PhoneNumberTypeMapping : ClassMap<PhoneNumberType>
        {
            public PhoneNumberTypeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }
    }
}
