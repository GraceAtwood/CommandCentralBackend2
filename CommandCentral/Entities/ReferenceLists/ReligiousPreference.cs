using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Religious Preference
    /// </summary>
    public class ReligiousPreference : ReferenceListItemBase
    {
        /// <summary>
        /// Maps a Religious Preference to the database.
        /// </summary>
        public class ReligiousPreferenceMapping : SubclassMap<ReligiousPreference>
        {
        }
    }
}