using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.AccountHistoryEvent
{
    public class Get
    {
        public Guid Id { get; set; }
        public Guid AccountHistoryEventType { get; set; }
        public DateTime EventTime { get; set; }
        public Guid Person { get; set; }
    }
}
