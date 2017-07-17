using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Type;
using CommandCentral.Utilities.Types;

namespace CommandCentral.Entities
{
    /// <summary>
    /// A comment.  It's assigned to an object where users can comment on it.
    /// </summary>
    public class Comment : Entity
    {

        #region Properties
        
        /// <summary>
        /// The person who created this comment.
        /// </summary>
        public virtual Person Creator { get; set; }

        /// <summary>
        /// The entity that owns this object.
        /// </summary>
        public virtual CommentableEntity OwningEntity { get; set; }

        /// <summary>
        /// This is the text of the comment.
        /// </summary>
        public virtual string Body { get; set; }

        /// <summary>
        /// The datetime at which this comment was made.
        /// </summary>
        public virtual DateTime TimeCreated { get; set; }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class CommentMapping : ClassMap<Comment>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public CommentMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Creator);

                Map(x => x.Body).Length(1000).Not.Nullable();
                Map(x => x.TimeCreated).Not.Nullable().CustomType<UtcDateTimeType>();

                ReferencesAny(x => x.OwningEntity)
                    .AddMetaValue<NewsItem>(typeof(NewsItem).Name)
                    //Uncomment this and the line below when adding comments to a Person breaks.  This is an experiment to make sure I understand this shit.
                    //.AddMetaValue<Person>(typeof(Person).Name)
                    .IdentityType<Guid>()
                    .EntityTypeColumn("OwningEntity_Type")
                    .EntityIdentifierColumn("OwningEntity_id")
                    .MetaType<string>();
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class CommentValidator : AbstractValidator<Comment>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public CommentValidator()
            {
                RuleFor(x => x.Creator).NotEmpty();
                RuleFor(x => x.Body).NotEmpty().Length(1, 1000);
                RuleFor(x => x.TimeCreated).NotEmpty();
                RuleFor(x => x.OwningEntity).NotEmpty();
            }
        }
    }
}
