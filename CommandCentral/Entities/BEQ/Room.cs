using CommandCentral.Authorization;
using CommandCentral.Enums;
using CommandCentral.Framework;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;

namespace CommandCentral.Entities.BEQ
{
    /// <summary>
    /// Describes a room in a building where a person can live.
    /// </summary>
    public class Room : Entity
    {
        #region Properties

        /// <summary>
        /// The level (floor) where the room is located in the building.
        /// </summary>
        public virtual int Level { get; set; }

        /// <summary>
        /// The room number.
        /// </summary>
        public virtual int Number { get; set; }

        /// <summary>
        /// The person assigned to the room.
        /// </summary>
        public virtual Person PersonAssigned { get; set; }

        /// <summary>
        /// The building this room is in.
        /// </summary>
        public virtual Building Building { get; set; }

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
        public class RoomMapping : ClassMap<Room>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public RoomMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Level).Not.Nullable();
                Map(x => x.Number).Not.Nullable();

                References(x => x.PersonAssigned);
                References(x => x.Building).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<Room>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();

                RuleFor(x => x.Level).GreaterThanOrEqualTo(1).LessThanOrEqualTo(3);
                RuleFor(x => x.Number).GreaterThanOrEqualTo(100).LessThanOrEqualTo(400);
                RuleFor(x => x.PersonAssigned)
                    .Must(person => person == null || person.DutyStatus == DutyStatuses.Active);

                RuleFor(x => x.Building).NotEmpty();
            }
        }

        /// <summary>
        /// Declares permissions for this object.
        /// </summary>
        public class Contract : RulesContract<Room>
        {
            /// <summary>
            /// Declares permissions for this object.
            /// </summary>
            public Contract()
            {
                RulesFor()
                    .CanEdit((person, room) =>
                        person.IsInChainOfCommandAtLevel(ChainsOfCommand.BEQ, ChainOfCommandLevels.Command))
                    .CanReturn((person, room) => room.PersonAssigned == null
                        ? person.IsInChainOfCommandAtLevel(ChainsOfCommand.BEQ, ChainOfCommandLevels.Command)
                        : person == room.PersonAssigned || person.IsInChainOfCommand(room.PersonAssigned));
            }
        }
    }
}