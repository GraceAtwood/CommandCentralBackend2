using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs
{
    public class DateTimeRangeQuery
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
