using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Criterion;
using CommandCentral.Framework;
using FluentValidation.Results;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Department and all of its divisions.
    /// </summary>
    public class Department : IValidatable
    {
        #region Properties
        
        public virtual Guid Id { get; set; }
        
        public virtual string Value { get; set; }

        public virtual string Description { get; set; }


        /// The command to which this department belongs.
        /// </summary>
        public virtual Command Command { get; set; }

        /// <summary>
        /// A list of those divisions that belong to this department.
        /// </summary>
        public virtual IList<Division> Divisions { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates this department object.
        /// </summary>
        /// <returns></returns>
        public virtual ValidationResult Validate()
        {
            return new DepartmentValidator().Validate(this);
        }
        
        #endregion

        /// <summary>
        /// Maps a department to the database.
        /// </summary>
        public class DepartmentMapping : ClassMap<Department>
        {
            /// <summary>
            /// Maps a department to the database.
            /// </summary>
            public DepartmentMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                HasMany(x => x.Divisions).Not.LazyLoad().Cascade.DeleteOrphan();

                References(x => x.Command).LazyLoad(Laziness.False);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the Department.
        /// </summary>
        public class DepartmentValidator : AbstractValidator<Department>
        {
            /// <summary>
            /// Validates the Department.
            /// </summary>
            public DepartmentValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a department must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }
    }
}
