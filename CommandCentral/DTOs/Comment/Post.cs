using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.Comment
{
    public class Post : Patch
    {
        public Guid OwningEntity { get; set; }
    }
}
