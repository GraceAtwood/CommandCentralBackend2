using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Utilities.Types;
using CommandCentral.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Type;

namespace CommandCentral.Entities.Muster
{
    public class StatusPeriod : CommentableEntity
    {

        #region Properties

        public virtual Person Person { get; set; }

        public virtual Person SubmittedBy { get; set; }

        public virtual DateTime DateSubmitted { get; set; }

        public virtual Person LastModifiedBy { get; set; }

        public virtual DateTime DateLastModified { get; set; }

        public virtual bool ExemptsFromWatch { get; set; }

        public virtual TimeRange Range { get; set; }

        public virtual StatusPeriodReason Reason { get; set; }


        #endregion

        #region CommentableEntity Members  

        public override IList<Comment> Comments { get; set; }

        public override bool CanPersonAccessComments(Person person)
        {
            return person.Equals(this.Person) || person.IsInChainOfCommand(this.Person);
        }

        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        #endregion

        public class StatusPeriodMapping : ClassMap<StatusPeriod>
        {
            public StatusPeriodMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.DateSubmitted).Not.Nullable();
                Map(x => x.DateLastModified).Not.Nullable();
                Map(x => x.ExemptsFromWatch).Not.Nullable();
                Component(x => x.Range, map =>
                {
                    map.Map(x => x.End).Not.Nullable().CustomType<UtcDateTimeType>();
                    map.Map(x => x.Start).Not.Nullable().CustomType<UtcDateTimeType>();
                });

                References(x => x.Person).Not.Nullable().Column("Person_id");
                References(x => x.SubmittedBy).Not.Nullable();
                References(x => x.LastModifiedBy).Not.Nullable();
                References(x => x.Reason).Not.Nullable();
            }
        }

        public class Validator : AbstractValidator<StatusPeriod>
        {
            public Validator()
            {
                RuleFor(x => x.Person).NotEmpty();
                RuleFor(x => x.SubmittedBy).NotEmpty();
                RuleFor(x => x.DateSubmitted).NotEmpty().InclusiveBetween(DateTime.MinValue, DateTime.UtcNow);
                RuleFor(x => x.LastModifiedBy).NotEmpty();
                RuleFor(x => x.DateLastModified).NotEmpty();
                RuleFor(x => x.Range)
                    .Must(range => range.Start <= range.End && range.Start != default(DateTime) && range.End != default(DateTime))
                        .WithMessage("A status period must start before it ends.")
                    .Must((period, range) => range.Start >= period.DateSubmitted)
                        .WithMessage("A status period must start after or at the same time it was submitted.  For example, you may not submit a retroactive status period.")
                    .Must((period, range) => range.End >= period.DateLastModified)
                        .WithMessage("A status period must end after it was submitted or after it was last modified.  For example, you may not modify a status period to end before now.");

                RuleFor(x => x.Reason).NotEmpty();
            }
        }
    }
}
