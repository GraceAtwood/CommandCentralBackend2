using System;
using System.Data;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;

namespace CommandCentral.Entities.CFS
{
    public class Meeting : Entity
    {
        #region Properties

        public virtual DateTime Time { get; set; }

        public virtual Person Person { get; set; }

        public virtual Person Advisor { get; set; } //go

        public virtual string Notes { get; set; }

        public virtual Request Request { get; set; }

        #endregion
        
        public override ValidationResult Validate()
        {
            throw new System.NotImplementedException();
        }

        public class Validator : AbstractValidator<Meeting>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Time).NotEmpty();
                RuleFor(x => x.Person).NotEmpty();
                RuleFor(x => x.Advisor).NotEmpty();
                RuleFor(x => x.Notes).Length(0, 1000);
                RuleFor(x => x.Request).NotEmpty();
            }
        }

        public class MeetingMapping : ClassMap<Meeting>
        {
            public MeetingMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Time).Not.Nullable();
                Map(x => x.Notes).Length(1000);

                References(x => x.Person).Not.Nullable();
                References(x => x.Advisor).Not.Nullable();
                References(x => x.Request).Not.Nullable();
            }
        }
    }
}