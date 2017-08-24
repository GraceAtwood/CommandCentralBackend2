﻿using System;
using System.Linq;
using FluentNHibernate.Mapping;
using FluentValidation;
using CommandCentral.Utilities;
using FluentValidation.Results;
using CommandCentral.Framework.Data;
using NHibernate.Linq;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single email address along with its data access methods
    /// </summary>
    public class EmailAddress : Entity
    {

        #region Properties

        /// <summary>
        /// The actual email address of this object.
        /// </summary>
        public virtual string Address { get; set; }

        /// <summary>
        /// Indicates whether or not a person is ok with releasing this email address outside their chain of command.
        /// </summary>
        public virtual bool IsReleasableOutsideCoC { get; set; }

        /// <summary>
        /// Indicates whether or not the client prefers to be contacted using this email address.
        /// </summary>
        public virtual bool IsPreferred { get; set; }

        /// <summary>
        /// The person who owns this email address.
        /// </summary>
        public virtual Person Person { get; set; }

        #endregion

        /// <summary>
        /// Returns a validation result for this object.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Validator().Validate(this);
        }

        /// <summary>
        /// Indicates whether or not this email address is a mail.mil email address.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsDoDEmailAddress()
        {
            var elements = Address.Split(new[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
            return elements.Any() && elements.Last().InsensitiveEquals("mail.mil");
        }
        
        /// <summary>
        /// Maps an email address to the database.
        /// </summary>
        public class EmailAddressMapping : ClassMap<EmailAddress>
        {
            /// <summary>
            /// Maps an email address to the database.
            /// </summary>
            public EmailAddressMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Address).Not.Nullable().Unique();
                Map(x => x.IsReleasableOutsideCoC).Not.Nullable();
                Map(x => x.IsPreferred).Not.Nullable();

                References(x => x.Person).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates an Email address
        /// </summary>
        public class Validator : AbstractValidator<EmailAddress>
        {
            /// <summary>
            /// Validates an Email address
            /// </summary>
            public Validator()
            {
                RuleFor(x => x.Address).EmailAddress().Must((item, address) =>
                {
                    return SessionManager.CurrentSession().Query<EmailAddress>().Count(x => x.Id != item.Id && x.Address == address) == 0;
                })
                .WithMessage("Email addresses must be unique.");
            }
        }
    }
}
