using System;
using System.Linq;
using FluentNHibernate.Mapping;
using FluentValidation;
using System.Collections.Generic;
using CommandCentral.Utilities;
using CommandCentral.Authorization.Rules;

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
        /// Indicates whether or not a person wants to be contacted using this email address.
        /// </summary>
        public virtual bool IsContactable { get; set; }

        /// <summary>
        /// Indicates whether or not the client prefers to be contacted using this email address.
        /// </summary>
        public virtual bool IsPreferred { get; set; }

        /// <summary>
        /// The person who owns this email address.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// Indicates whether or not this email address is a mail.mil email address.  This is a calculated field, built using the Address field.
        /// </summary>
        public virtual bool IsDodEmailAddress
        {
            get
            {
                var elements = Address.Split(new[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                if (!elements.Any())
                    return false;

                return elements.Last().SafeEquals("mail.mil");
            }
        }

        #endregion

        #region Overrides
        
        /// <summary>
        /// Returns a string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<string> preferences = new List<string>();
            if (IsContactable)
                preferences.Add("C");
            if (IsPreferred)
                preferences.Add("P");
            
            string final = preferences.Any() ? $"({String.Join("|", preferences)})" : "";

            return $"{Address} {final}";
        }

        #endregion

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
                Map(x => x.IsContactable).Not.Nullable();
                Map(x => x.IsPreferred).Not.Nullable();

                References(x => x.Person).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates an Email address
        /// </summary>
        public class EmailAddressValidator : AbstractValidator<EmailAddress>
        {
            /// <summary>
            /// Validates an Email address
            /// </summary>
            public EmailAddressValidator()
            {
                RuleFor(x => x.Address).Must(x =>
                    {
                        try
                        {
                            var address = new System.Net.Mail.MailAddress(x);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    });
            }
        }
    }
}
