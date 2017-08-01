using System.Linq;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using FluentValidation;
using NHibernate.Criterion;
using CommandCentral.Authorization;
using FluentValidation.Results;
using CommandCentral.Framework;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single command, such as NIOC GA and all of its departments and divisions.
    /// </summary>
    public class Command : Entity
    {
        #region Properties

        /// <summary>
        /// The name of this command.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// A brief description of this command.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The departments of the command
        /// </summary>
        public virtual IList<Department> Departments { get; set; } = new List<Department>();

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates this command object.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }
        
        #endregion

        /// <summary>
        /// Maps a command to the database.
        /// </summary>
        public class CommandMapping : ClassMap<Command>
        {
            /// <summary>
            /// Maps a command to the database.
            /// </summary>
            public CommandMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Name).Not.Nullable().Unique();
                Map(x => x.Description);

                HasMany(x => x.Departments).Cascade.All();

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the Command.
        /// </summary>
        public class Validator : AbstractValidator<Command>
        {
            /// <summary>
            /// Validates the Command.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a Command must be no more than 255 characters.");
                RuleFor(x => x.Name).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }

    }
}
