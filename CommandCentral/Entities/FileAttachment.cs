using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using FluentNHibernate.Mapping;
using System.IO;
using CommandCentral.Framework;

namespace CommandCentral.Entities
{
    /// <summary>
    /// A file attachment is a binary file of some kind with an overlay image on which clients can draw.
    /// </summary>
    public class FileAttachment : Entity
    {
        /// <summary>
        /// The path to the attachments directory as set in the .json app settings.
        /// </summary>
        public static string AttachmentsDirectory = Utilities.ConfigurationUtility.Configuration["Attachments"];

        /// <summary>
        /// The object that owns this file attachment.
        /// </summary>
        public virtual IHazAttachments OwningEntity { get; set; }

        /// <summary>
        /// The file extension for this attachment.
        /// </summary>
        public virtual string FileExtension { get; set; }

        /// <summary>
        /// Returns the file path for the attachment itself.
        /// </summary>
        public virtual string AttachmentFilePath => Path.Combine(Directory.GetCurrentDirectory(), AttachmentsDirectory, Id + ".ccatt");

        /// <summary>
        /// Returns the file path to the overlay image for this file attachment.
        /// </summary>
        public virtual string OverlayFilePath => Path.Combine(Directory.GetCurrentDirectory(), AttachmentsDirectory, Id + ".ccover");

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class FileAttachmentMapping : ClassMap<FileAttachment>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public FileAttachmentMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.FileExtension).Not.Nullable();

                ReferencesAny(x => x.OwningEntity)
                    .AddMetaValue<Correspondence.CorrespondenceItem>(typeof(Correspondence.CorrespondenceItem).Name)
                    //Uncomment this and the line below when adding comments to a Person breaks.  This is an experiment to make sure I understand this shit.
                    //.AddMetaValue<Person>(typeof(Person).Name)
                    .IdentityType<Guid>()
                    .EntityTypeColumn("OwningEntity_Type")
                    .EntityIdentifierColumn("OwningEntity_id")
                    .MetaType<string>();
            }
        }
    }
}
