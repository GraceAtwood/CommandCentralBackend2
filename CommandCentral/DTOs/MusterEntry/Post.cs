using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.MusterEntry
{
    public class Post : Patch
    {
        public Guid Person { get; set; }
    }
}
