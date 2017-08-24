using CommandCentral.Entities;
using System;
using System.Collections.Generic;

namespace CommandCentral.Framework
{
    public interface IHazAttachments
    {
        Guid Id { get; set; }

        bool CanPersonAccessAttachments(Person person);

        IList<FileAttachment> Attachments { get; set; }
    }
}
