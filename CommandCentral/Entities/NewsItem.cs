﻿using System;
using System.Linq;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using FluentValidation;
using CommandCentral.Authorization;
using NHibernate.Type;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single News Item and its members, including its DB access members.
    /// </summary>
    public class NewsItem : IEntity
    {

        #region Properties

        /// <summary>
        /// The Id of the news item.
        /// </summary>
        public virtual Guid Id { get; set; }

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

        #endregion

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
            }
        }

        /// <summary>
        /// Validates the properties of a news item.
        /// </summary>
        public class NewsItemValidator : AbstractValidator<NewsItem>
        {
            /// <summary>
            /// Validates the properties of a news item.
            /// </summary>
            public NewsItemValidator()
            {
                RuleFor(x => x.CreationTime).NotEmpty();
                RuleFor(x => x.Creator).NotEmpty();
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Body).Length(10, 3500);
                RuleFor(x => x.Title).NotEmpty().Length(3, 50).WithMessage("The title must not be blank and must be between 3 and 50 characters.");
            }
        }
    }
}
