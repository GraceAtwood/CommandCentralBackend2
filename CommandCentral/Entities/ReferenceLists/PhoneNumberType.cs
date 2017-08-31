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

        public class PhoneNumberTypeMapping : SubclassMap<PhoneNumberType>
        {
        }
    }
}