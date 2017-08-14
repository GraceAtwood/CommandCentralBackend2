using CommandCentral.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.AccountHistoryEvent
{
    public class Get
    {
        public Guid Id { get; set; }
        public AccountHistoryTypes AccountHistoryEventType { get; set; }
        public DateTime EventTime { get; set; }
        public Guid Person { get; set; }

        public Get(Entities.AccountHistoryEvent item)
        {
            Id = item.Id;
            AccountHistoryEventType = item.AccountHistoryEventType;
            EventTime = item.EventTime;
            Person = item.Person.Id;
        }
    }
}
