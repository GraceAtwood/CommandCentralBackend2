using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using CommandCentral.Framework;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Religious Preference
    /// </summary>
    [EditableReferenceList]
    public class ReligiousPreference : ReferenceListItemBase
    {
        /// <summary>
        /// Validates this religious preference.  
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new ReligiousPreferenceValidator().Validate(this);
        }

        /// <summary>
        /// Maps a Religious Preference to the database.
        /// </summary>
        public class ReligiousPreferenceMapping : SubclassMap<ReligiousPreference>
        {
        }

        /// <summary>
        /// Validates the RP
        /// </summary>
        public class ReligiousPreferenceValidator : AbstractValidator<ReligiousPreference>
        {
            /// <summary>
            /// Validates the RP
            /// </summary>
            public ReligiousPreferenceValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("A religious preference's description may be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }
    }
}