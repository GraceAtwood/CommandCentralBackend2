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

        public bool HasFromNotTo()
        {
            return From.HasValue && !To.HasValue;
        }

        public bool HasToNotFrom()
        {
            return To.HasValue && !From.HasValue;
        }

        public bool HasBoth()
        {
            return To.HasValue && From.HasValue;
        }

        public bool HasNeither()
        {
            return !To.HasValue && !From.HasValue;
        }
    }
}
