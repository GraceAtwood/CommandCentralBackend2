using CommandCentral.Enums;
using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CommandCentral.Entities.Watchbill
{
    public class WatchShiftType : Entity
    {
        /// <summary>
        /// The Name of this shift
        /// </summary>
        public virtual string Name { get; set; }
        
        /// <summary>
        /// The optional description of this shift
        /// </summary>
        public virtual string Description { get; set; }
        
        /// <summary>
        /// The qualification required to stand this shift
        /// </summary>
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