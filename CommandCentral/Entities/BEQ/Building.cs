using System.Collections.Generic;
using CommandCentral.Authorization;
using CommandCentral.Enums;
using CommandCentral.Framework;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;

namespace CommandCentral.Entities.BEQ
{
    /// <summary>
    /// Describes a building in which people can live owned by the command.
    /// </summary>
    public class Building : Entity
    {
        #region Properties

        /// <summary>
        /// The name of the building.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// A brief description of  this building.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The collection of rooms in this building.
        /// </summary>
        public virtual IList<Room> Rooms { get; set; }

        /// <summary>
        /// The command that owns this building.
        /// </summary>
        public virtual Command Command { get; set; }

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
        public class BuildingMapping : ClassMap<Building>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public BuildingMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Name).Not.Nullable();
                Map(x => x.Description).Not.Nullable();

                References(x => x.Command).Not.Nullable();

                HasMany(x => x.Rooms).Cascade.All();
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<Building>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();

                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Command).NotEmpty();
            }
        }

        /// <summary>
        /// Declares permissions for this object.
        /// </summary>
        public class Contract : RulesContract<Building>
        {
            /// <summary>
            /// Declares permissions for this object.
            /// </summary>
            public Contract()
            {
                RulesFor()
                    .CanEdit((person, building) =>
                        person.IsInChainOfCommandAtLevel(ChainsOfCommand.BEQ, ChainOfCommandLevels.Command))
                    .CanReturn((person, building) =>
                        person.IsInChainOfCommandAtLevel(ChainsOfCommand.BEQ, ChainOfCommandLevels.Command));
            }
        }
    }
}