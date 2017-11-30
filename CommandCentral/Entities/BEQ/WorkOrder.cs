using System;
using CommandCentral.Authorization;
using CommandCentral.Enums;
using CommandCentral.Framework;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using NHibernate.Type;

namespace CommandCentral.Entities.BEQ
{
    /// <summary>
    /// A work order submitted for something to be fixed.
    /// </summary>
    public class WorkOrder : CommentableEntity
    {
        #region Properties

        /// <summary>
        /// The body of the work order decribes what needs to be fixed.
        /// </summary>
        public virtual string Body { get; set; }

        /// <summary>
        /// A brief description of where the thing is that needs to be fixed.
        /// </summary>
        public virtual string Location { get; set; }

        /// <summary>
        /// If the work order can be located in a room, this is set.  Otherwise, leave it blank.
        /// </summary>
        public virtual Room RoomLocation { get; set; }

        /// <summary>
        /// The person who submitted this work order.
        /// </summary>
        public virtual Person SubmittedBy { get; set; }

        /// <summary>
        /// The time at which this work order was submitted.
        /// </summary>
        public virtual DateTime TimeSubmitted { get; set; }

        #endregion

        /// <summary>
        /// Validates this object.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WorkOrderMapping : ClassMap<WorkOrder>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WorkOrderMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Body).Not.Nullable().Length(2048);
                Map(x => x.Location).Not.Nullable();
                Map(x => x.TimeSubmitted).Not.Nullable().CustomType<UtcDateTimeType>();

                References(x => x.RoomLocation);
                References(x => x.SubmittedBy).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<WorkOrder>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Body).NotEmpty().Length(1, 2048);

                RuleFor(x => x.Location).NotEmpty();

                RuleFor(x => x.SubmittedBy).NotEmpty();
                RuleFor(x => x.TimeSubmitted).NotEmpty();
            }
        }

        /// <summary>
        /// Declares permissions for this object.
        /// </summary>
        public class Contract : RulesContract<WorkOrder>
        {
            /// <summary>
            /// Declares permissions for this object.
            /// </summary>
            public Contract()
            {
                RulesFor()
                    .CanEdit((person, order) =>
                        person.IsInChainOfCommandAtLevel(ChainsOfCommand.BEQ, ChainOfCommandLevels.Command))
                    .CanReturn((person, order) =>
                        person.IsInChainOfCommandAtLevel(ChainsOfCommand.BEQ, ChainOfCommandLevels.Command));
            }
        }
    }
}