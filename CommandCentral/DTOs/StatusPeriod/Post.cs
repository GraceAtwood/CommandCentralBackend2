using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.StatusPeriod
{
    public class Post : Put
    {
        public Guid Person { get; set; }
    }
}
