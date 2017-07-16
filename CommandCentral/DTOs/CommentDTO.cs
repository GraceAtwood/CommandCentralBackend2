using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs
{
    public class CommentDTO
    {
        public Guid Id { get; set; }
        public Guid Creator { get; set; }
        public Guid OwningEntity { get; set; }
        public string Body { get; set; }
        public DateTime TimeCreated { get; set; }
    }
}
