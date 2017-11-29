using System.Collections.Generic;
using System.Linq;
using CommandCentral.Enums;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;

namespace CommandCentral.Entities.CollateralDutyTracking
{
    /// <summary>
    /// A collateral duty such as FHD, watchbill coordinator, etc.  
    /// Not intended to replace the permissions system and its groups.
    /// </summary>
    public class CollateralDuty : CommentableEntity
    {
        #region Properties

        /// <summary>
        /// The name of this collateral duty.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The chain of command that this collateral duty represents.
        /// </summary>
        public virtual ChainsOfCommand ChainOfCommand { get; set; }

        /// <summary>
        /// The command at which this collateral duty exists.
        /// </summary>
        public virtual Command Command { get; set; }

        /// <summary>
        /// The list of memberships to this collateral duty.
        /// </summary>
        public virtual IList<CollateralDutyMembership> Membership { get; set; }
        
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
        /// Determines if the given person can access the comments on this collateral duty.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool CanPersonAccessComments(Person person) => true;

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class CollateralDutyMapping : ClassMap<CollateralDuty>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public CollateralDutyMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Name).Not.Nullable();

                References(x => x.Command).Not.Nullable();

                HasMany(x => x.Membership).Cascade.AllDeleteOrphan();
                
                HasMany(x => x.Comments)
                    .Cascade.AllDeleteOrphan()
                    .KeyColumn("OwningEntity_id")
                    .ForeignKeyConstraintName("none");
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<CollateralDuty>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();

                RuleFor(x => x.Command).NotEmpty();

                RuleFor(x => x.Name).NotEmpty()
                    .Must((duty, name) =>
                        SessionManager.GetCurrentSession().Query<CollateralDuty>()
                            .Count(y => y.Id != duty.Id && y.Name == name) == 0)
                    .WithMessage("The name of a collateral duty must be unique.");
            }
        }
    }
}