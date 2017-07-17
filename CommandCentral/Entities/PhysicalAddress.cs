using System;
using System.IO;
using System.Net;
using FluentNHibernate.Mapping;
using FluentValidation;
using CommandCentral.Framework.Data;
using CommandCentral.Enums;
using CommandCentral.Utilities;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single physical address
    /// </summary>
    public class PhysicalAddress : Entity
    {

        #region Properties

        /// <summary>
        /// The street number + route address.
        /// </summary>
        public virtual string Address { get; set; }

        /// <summary>
        /// The city.
        /// </summary>
        public virtual string City { get; set; }

        /// <summary>
        /// The state.
        /// </summary>
        public virtual string State { get; set; }

        /// <summary>
        /// The zip code.
        /// </summary>
        public virtual string ZipCode { get; set; }

        /// <summary>
        /// The country.
        /// </summary>
        public virtual string Country { get; set; }

        /// <summary>
        /// Indicates whether or not the person lives at this address
        /// </summary>
        public virtual bool IsHomeAddress { get; set; }

        /// <summary>
        /// The person who owns this physical address.
        /// </summary>
        public virtual Person Person { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the address in this format: 123 Fake Street, Happyville, TX 54321
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{(IsHomeAddress ? "(Home) " : "")}{Address}, {City}, {State} {ZipCode}";
        }
        
        #endregion

        /// <summary>
        /// Maps a physical address to the database.
        /// </summary>
        public class PhysicalAddressMapping : ClassMap<PhysicalAddress>
        {
            /// <summary>
            /// Maps a physical address to the database.
            /// </summary>
            public PhysicalAddressMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Address).Not.Nullable();
                Map(x => x.City).Not.Nullable();
                Map(x => x.State).Not.Nullable();
                Map(x => x.ZipCode).Not.Nullable();
                Map(x => x.Country);
                Map(x => x.IsHomeAddress).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates a physical address
        /// </summary>
        public class PhysicalAddressValidator : AbstractValidator<PhysicalAddress>
        {
            /// <summary>
            /// Validates a physical address
            /// </summary>
            public PhysicalAddressValidator()
            {
                CascadeMode = CascadeMode.StopOnFirstFailure;
                
                RuleFor(x => x.Address)
                    .NotEmpty().WithMessage("Your address must not be empty.")
                    .Length(1, 255).WithMessage("The address must be between 1 and 255 characters.");

                RuleFor(x => x.City)
                    .NotEmpty().WithMessage("Your city must not be empty.")
                    .Length(1, 255).WithMessage("The city must be between 1 and 255 characters.");

                RuleFor(x => x.State)
                    .NotEmpty().WithMessage("Your state must not be empty.")
                    .Length(1, 255).WithMessage("The state must be between 1 and 255 characters.");

                RuleFor(x => x.Country)
                    .Length(0, 255).WithMessage("The country may be no more than 200 characters.");

                RuleFor(x => x.ZipCode)
                    .NotEmpty().WithMessage("You zip code must not be empty.")
                    .Matches(@"^\d{5}(?:[-\s]\d{4})?$").WithMessage("Your zip code was not valid.");
            }
        }

        /// <summary>
        /// Provides searching strategies.
        /// </summary>
        public class PhysicalAddressQueryProvider : QueryStrategyProvider<PhysicalAddress>
        {
            /// <summary>
            /// Provides searching strategies.
            /// </summary>
            public PhysicalAddressQueryProvider()
            {
                ForProperties(
                    x => x.Id)
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    return CommonQueryStrategies.IdQuery(token.SearchParameter.Key.GetPropertyName(), token.SearchParameter.Value);
                });

                ForProperties(
                    x => x.Address,
                    x => x.City,
                    x => x.State,
                    x => x.ZipCode,
                    x => x.Country)
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced, QueryTypes.Simple)
                .UsingStrategy(token =>
                {
                    return CommonQueryStrategies.StringQuery(token.SearchParameter.Key.GetPropertyName(), token.SearchParameter.Value);
                });

                ForProperties(
                    x => x.IsHomeAddress)
                .AsType(SearchDataTypes.Boolean)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    return CommonQueryStrategies.BooleanQuery(token.SearchParameter.Key.GetPropertyName(), token.SearchParameter.Value);
                });
            }
        }
    }
}
