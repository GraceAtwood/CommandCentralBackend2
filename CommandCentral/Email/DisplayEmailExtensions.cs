using CommandCentral.Entities;

namespace CommandCentral.Email
{
    /// <summary>
    /// A set of extensions meant to assist the email module in rendering objects.
    /// </summary>
    public static class DisplayEmailExtensions
    {
        /// <summary>
        /// Returns a string that is suitable for displaying and identifying this person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static string ToDisplayName(this Person person)
        {
            return $"{person.LastName}, {person.FirstName} {person.MiddleName}";
        }
    }
}