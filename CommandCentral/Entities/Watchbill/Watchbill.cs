using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CommandCentral.Entities.Watchbill
{
    public class Watchbill : Entity
    {
        public class WatchbillMapping : ClassMap<Watchbill>
        {
            
        }
        public override ValidationResult Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}