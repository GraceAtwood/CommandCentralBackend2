using CommandCentral.Email.Models;

namespace CommandCentral.Email
{
    /// <summary>
    /// A list of all templates used throughout the application.
    /// </summary>
    public static class Templates
    {
        /// <summary>
        /// The template meant to be used for the correspondence modified email.
        /// </summary>
        public static readonly CCEmailTemplate<CorrespondenceModified> CorrespondenceModifiedTemplate = new CCEmailTemplate<CorrespondenceModified>("CorrespondenceModified.cshtml");
    }
}