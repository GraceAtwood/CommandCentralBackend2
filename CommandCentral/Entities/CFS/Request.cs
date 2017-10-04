using System;
using CommandCentral.Entities.ReferenceLists;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using NHibernate.Type;

namespace CommandCentral.Entities.CFS
{
    public class Request : Entity
    {
        #region Properties
        
        public virtual DateTime TimeSubmitted { get; set; }

        public virtual Person Person { get; set; }

        public virtual CFSRequestType RequestType { get; set; }

        public virtual Person ClaimedBy { get; set; }

        #endregion

        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.TimeSubmitted).NotEmpty();
                RuleFor(x => x.Person).NotEmpty();
                RuleFor(x => x.RequestType).NotEmpty();
            }
        }

        public class RequestMapping : ClassMap<Request>
        {
            public RequestMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.TimeSubmitted).Not.Nullable().CustomType<UtcDateTimeType>();

                References(x => x.Person).Not.Nullable();
                References(x => x.RequestType).Not.Nullable();
                References(x => x.ClaimedBy);
            }
        }
    }
}