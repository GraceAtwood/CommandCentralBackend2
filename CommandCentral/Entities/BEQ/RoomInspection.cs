using System;
using System.Collections.Generic;
using System.Linq;
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
    /// A room inspection submitted for a single room.
    /// </summary>
    public class RoomInspection : CommentableEntity
    {
        #region Properties

        /// <summary>
        /// The date/time at which this room inspection was done.
        /// </summary>
        public virtual DateTime Time { get; set; }

        /// <summary>
        /// The room that was inspected.
        /// </summary>
        public virtual Room Room { get; set; }

        /// <summary>
        /// The peson who was living in this room at the time it was inspected.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// A collection of those people who conducted the inspection.
        /// </summary>
        public virtual IList<Person> InspectedBy { get; set; }

        /// <summary>
        /// The score the person received.
        /// </summary>
        public virtual int Score { get; set; }

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
        public class RoomInspectionMapping : ClassMap<RoomInspection>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public RoomInspectionMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Time).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.Score).Not.Nullable();

                References(x => x.Room).Not.Nullable();
                References(x => x.Person).Not.Nullable();

                HasMany(x => x.InspectedBy);
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<RoomInspection>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();

                RuleFor(x => x.Time).NotEmpty();
                RuleFor(x => x.Room).NotEmpty();
                RuleFor(x => x.Person).NotEmpty();

                RuleFor(x => x.InspectedBy).Must(list => list != null && list.Count > 0);

                RuleFor(x => x.Score).GreaterThanOrEqualTo(0).LessThanOrEqualTo(25);
            }
        }

        /// <summary>
        /// Declares permissions for this object.
        /// </summary>
        public class Contract : RulesContract<RoomInspection>
        {
            /// <summary>
            /// Declares permissions for this object.
            /// </summary>
            public Contract()
            {
                RulesFor()
                    .CanEdit((person, inspection) =>
                        person.IsInChainOfCommandAtLevel(ChainsOfCommand.BEQ, ChainOfCommandLevels.Command))
                    .CanReturn((person, inspection) =>
                    {
                        if (inspection.Person == null)
                            return person.IsInChainOfCommandAtLevel(ChainsOfCommand.BEQ, ChainOfCommandLevels.Command);

                        return person == inspection.Person || person.IsInChainOfCommand(inspection.Person) ||
                               inspection.InspectedBy.Contains(person) ||
                               inspection.InspectedBy.Any(x => person.IsInChainOfCommand(x));
                    });
            }
        }
    }
}