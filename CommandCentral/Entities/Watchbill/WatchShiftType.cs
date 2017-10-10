using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CommandCentral.Entities.Watchbill
{
    public class WatchShiftType : Entity
    {
        public class WatchShiftTypeMapping : ClassMap<WatchShiftType>
        {
            
        }
        public override ValidationResult Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}