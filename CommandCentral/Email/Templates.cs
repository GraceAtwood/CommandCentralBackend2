using CommandCentral.Email.Models;

namespace CommandCentral.Email
{
    /// <summary>
    /// A list of all templates used throughout the application.
    /// </summary>
    public static class Templates
    {
        /// <summary>
        /// The template meant to be used for the correspondence completed email.
        /// </summary>
        public static readonly CCEmailTemplate<CorrespondenceGeneric> CorrespondenceCompletedTemplate = new CCEmailTemplate<CorrespondenceGeneric>("CorrespondenceCompleted.cshtml");
        
        /// <summary>
        /// The template meant to be used for the correspondence created email.
        /// </summary>
        public static readonly CCEmailTemplate<CorrespondenceGeneric> CorrespondenceCreatedTemplate = new CCEmailTemplate<CorrespondenceGeneric>("CorrespondenceCreated.cshtml");
        
        /// <summary>
        /// The template meant to be used for the correspondence deleted email.
        /// </summary>
        public static readonly CCEmailTemplate<CorrespondenceGeneric> CorrespondenceDeletedTemplate = new CCEmailTemplate<CorrespondenceGeneric>("CorrespondenceDeleted.cshtml");
        
        /// <summary>
        /// The template meant to be used for the correspondence modified email.
        /// </summary>
        public static readonly CCEmailTemplate<CorrespondenceGeneric> CorrespondenceModifiedTemplate = new CCEmailTemplate<CorrespondenceGeneric>("CorrespondenceModified.cshtml");
    }
}