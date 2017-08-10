using CommandCentral.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral
{
    public interface IHazAttachments
    {
        Guid Id { get; set; }

        bool CanPersonAccessAttachments(Person person);

        IList<FileAttachment> Attachments { get; set; }
    }
}
