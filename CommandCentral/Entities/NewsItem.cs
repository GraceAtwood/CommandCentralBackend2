using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using CommandCentral.Framework;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single News Item and its members, including its DB access members.
    /// </summary>
    public class NewsItem : Entity, IHazComments
    {
        #region Properties
        
        /// <summary>
        /// The client that created the news item.
        /// </summary>
        public virtual Person Creator { get; set; }

        /// <summary>
        /// The title of the news item.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// The body of the news item.
        /// </summary>
        public virtual string Body { get; set; }

        /// <summary>
        /// The time this news item was created.
        /// </summary>
        public virtual DateTime CreationTime { get; set; }

        /// <summary>
        /// Comments for this news item.
        /// </summary>
        public virtual IList<Comment> Comments { get; set; }
        
        #endregion

        /// <summary>
        /// Determine if a person can access the comments.  For news item, everyone can.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool CanPersonAccessComments(Person person)
        {
            return true;
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Maps a news item to the database.
        /// </summary>
        public class NewsItemMapping : ClassMap<NewsItem>
        {
            /// <summary>
            /// Maps a news item to the database.
            /// </summary>
            public NewsItemMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Creator).Not.Nullable();

                Map(x => x.Title).Not.Nullable().Length(50);
                Map(x => x.Body).Not.Nullable().Length(3500);
                Map(x => x.CreationTime).Not.Nullable();

                HasMany(x => x.Comments)
                    .Cascade.AllDeleteOrphan()
                    .KeyColumn("OwningEntity_id")
                    .ForeignKeyConstraintName("none");
            }
        }

        /// <summary>
        /// Validates the properties of a news item.
        /// </summary>
        public class Validator : AbstractValidator<NewsItem>
        {
            /// <summary>
            /// Validates the properties of a news item.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.CreationTime).NotEmpty();
                RuleFor(x => x.Creator).NotEmpty();
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Body).NotEmpty().Length(10, 3500);
                RuleFor(x => x.Title).NotEmpty().Length(3, 50);
            }
        }
    }
}
