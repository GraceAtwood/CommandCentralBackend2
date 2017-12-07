using System;
using CommandCentral.Authorization;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Enums;
using CommandCentral.Framework;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Type;
using FluentValidation.Results;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single change.
    /// </summary>
    public class Change : Entity
    {
        #region Properties

        /// <summary>
        /// The client who initiated this change.
        /// </summary>
        public virtual Person Editor { get; set; }

        /// <summary>
        /// The entity on which the property was modified.
        /// </summary>
        public virtual Entity Entity { get; set; }

        /// <summary>
        /// The path to the property that was modified.
        /// </summary>
        public virtual string PropertyPath { get; set; }

        /// <summary>
        /// The value prior to the update or change.
        /// </summary>
        public virtual string OldValue { get; set; }

        /// <summary>
        /// The new value.
        /// </summary>
        public virtual string NewValue { get; set; }

        /// <summary>
        /// The time this change was made.
        /// </summary>
        public virtual DateTime ChangeTime { get; set; }

        /// <summary>
        /// The type of change.
        /// </summary>
        public virtual ChangeTypes ChangeType { get; set; }

        #endregion

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Maps a change to the database.
        /// </summary>
        public class ChangeMapping : ClassMap<Change>
        {
            /// <summary>
            /// Maps a change to the database.
            /// </summary>
            public ChangeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Editor).Not.Nullable();

                Map(x => x.ChangeTime).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.PropertyPath).Not.Nullable();
                Map(x => x.OldValue);
                Map(x => x.NewValue);
                Map(x => x.ChangeType);

                ReferencesAny(x => x.Entity)
                    .AddMetaValue<NewsItem>(nameof(NewsItem))
                    .AddMetaValue<EmailAddress>(nameof(EmailAddress))
                    .AddMetaValue<Watchbill.Watchbill>(nameof(Watchbill.Watchbill))
                    .IdentityType<Guid>()
                    .EntityTypeColumn("Entity_Type")
                    .EntityIdentifierColumn("Entity_id")
                    .MetaType<string>();
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<Change>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.ChangeTime).NotEmpty();
                RuleFor(x => x.Editor).NotEmpty();
                RuleFor(x => x.Entity).NotEmpty();
                RuleFor(x => x).Must(x => !Equals(x.NewValue, x.OldValue))
                    .WithMessage("New and old values must be different.");
                RuleFor(x => x.PropertyPath).NotEmpty();
                RuleFor(x => x.Id).NotEmpty();
            }
        }

        /// <summary>
        /// Rules for this object.
        /// </summary>
        public class Contract : RulesContract<Change>
        {
            /// <summary>
            /// Rules for this object.
            /// </summary>
            public Contract()
            {
                RulesFor()
                    .CanEdit((person, change) => person.CanEdit(change.Entity, change.PropertyPath))
                    .CanReturn((person, change) => person.CanReturn(change.Entity, change.PropertyPath));
            }
        }
    }
}