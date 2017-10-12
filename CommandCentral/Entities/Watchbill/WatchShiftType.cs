using CommandCentral.Enums;
using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CommandCentral.Entities.Watchbill
{
    public class WatchShiftType : Entity
    {
        public virtual string Name { get; set; }
        
        public virtual string Description { get; set; }
        
        public virtual WatchQualifications Qualification { get; set; }
        
        public class WatchShiftTypeMapping : ClassMap<WatchShiftType>
        {
            public WatchShiftTypeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Name).Not.Nullable();
                Map(x => x.Description);
                Map(x => x.Qualification).CustomType<GenericEnumMapper<WatchQualifications>>();
            }
        }
        public override ValidationResult Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}