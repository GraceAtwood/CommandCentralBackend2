using System;
using System.Collections.Generic;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Framework;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using NHibernate.Type;

namespace CommandCentral.Entities.CFS
{
    /// <summary>
    /// Represents a single request for assistance from a user to the command financial specialists.
    /// </summary>
    public class Request : Entity
    {
        #region Properties
        
        /// <summary>
        /// The time at which the request was submitted.
        /// </summary>
        public virtual DateTime TimeSubmitted { get; set; }

        /// <summary>
        /// The person who submitted this request.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// The type of request.
        /// </summary>
        public virtual CFSRequestType RequestType { get; set; }

        /// <summary>
        /// The person from the CFS group that claimed this request.
        /// </summary>
        public virtual Person ClaimedBy { get; set; }

        /// <summary>
        /// Indicates that this request has been claimed.
        /// </summary>
        public virtual bool IsClaimed { get; set; }

        /// <summary>
        /// The collection of meetings that were held due to this request for help.
        /// </summary>
        public virtual IList<Meeting> Meetings { get; set; }

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
        /// Validates this object.
        /// </summary>
        public class Validator : AbstractValidator<Request>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.TimeSubmitted).NotEmpty();
                RuleFor(x => x.Person).NotEmpty();
                RuleFor(x => x.RequestType).NotEmpty();
            }
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class RequestMapping : ClassMap<Request>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public RequestMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.TimeSubmitted).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.IsClaimed).Not.Nullable();

                References(x => x.Person).Not.Nullable();
                References(x => x.RequestType).Not.Nullable();
                References(x => x.ClaimedBy);

                HasMany(x => x.Meetings);
            }
        }
    }
}