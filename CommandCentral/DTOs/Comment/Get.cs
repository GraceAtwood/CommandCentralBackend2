using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.Comment
{
    public class Get : Post
    {
        public Guid Id { get; set; }
        public Guid Creator { get; set; }
        public DateTime TimeCreated { get; set; }
    }
}
