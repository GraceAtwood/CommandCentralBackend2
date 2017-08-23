using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.Registration
{
    public class Get : Post
    {
        public DateTime TimeSubmitted { get; set; }
    }
}