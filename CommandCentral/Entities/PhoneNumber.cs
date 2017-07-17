using System;
using CommandCentral.Entities.ReferenceLists;
using FluentNHibernate.Mapping;
using FluentValidation;
using System.Linq;
using System.Collections.Generic;
using CommandCentral.Utilities;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single Phone number along with its data access members and properties
    /// </summary>
    public class PhoneNumber : Entity
    {
        #region Properties

        /// <summary>
        /// The actual phone number of this phone number object.
        /// </summary>
        public virtual string Number { get; set; }

        /// <summary>
        /// Indicates whether or not the person who owns this phone number wants any contact to occur using it.
        /// </summary>
        public virtual bool IsContactable { get; set; }

        /// <summary>
        /// Indicates whether or not the person who owns this phone number prefers contact to occur on it.
        /// </summary>
        public virtual bool IsPreferred { get; set; }

        /// <summary>
        /// The type of this phone. eg. Mobile, Home, Work
        /// </summary>
        public virtual PhoneNumberType PhoneType { get; set; }

        /// <summary>
        /// The person who owns this phone number.
        /// </summary>
        public virtual Person Person { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the Number property along with the user preferences printed next to it.
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

            char phoneType = PhoneType.Value.First();

            return $"{Number} ({phoneType}) {final}";
        }

        #endregion
        
        /// <summary>
        /// Maps a single phone number to the database.
        /// </summary>
        public class PhoneNumberMapping : ClassMap<PhoneNumber>
        {
            /// <summary>
            /// Maps a single phone number to the database.
            /// </summary>
            public PhoneNumberMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Number).Not.Nullable().Length(15);
                Map(x => x.IsContactable).Not.Nullable();
                Map(x => x.IsPreferred).Not.Nullable();

                References(x => x.PhoneType).Not.Nullable();
                References(x => x.Person).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates the phone number object.
        /// </summary>
        public class PhoneNumberValidator : AbstractValidator<PhoneNumber>
        {
            /// <summary>
            /// Validates the phone number object.
            /// </summary>
            public PhoneNumberValidator()
            {
                RuleFor(x => x.Number).Length(0, 10)
                    .Must(x => x.All(char.IsDigit))
                    .WithMessage("Your phone number must only be 10 digits.");

                RuleFor(x => x.PhoneType).NotEmpty()
                    .WithMessage("The phone number type must not be left blank.")
                    .Must(x => x.Id != Guid.Empty)
                    .WithMessage("The phone number type must not be left blank.");
            }
        }

    }
}
