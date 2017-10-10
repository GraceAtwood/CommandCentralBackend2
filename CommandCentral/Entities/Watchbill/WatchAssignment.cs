using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CommandCentral.Entities.Watchbill
{
    public class WatchAssignment : Entity
    {
        public class WatchAssignmentMapping : ClassMap<WatchAssignment>
        {
            
        }
        
        public override ValidationResult Validate()
        {
            throw new System.NotImplementedException();
        }

    }
}