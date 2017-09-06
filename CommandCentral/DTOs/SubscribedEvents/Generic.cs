using System.Collections.Generic;
using CommandCentral.Enums;

namespace CommandCentral.DTOs.SubscribedEvents
{
    public class Generic
    {
        public SubscribableEvents Event { get; set; }
        public ChainOfCommandLevels Level { get; set; }

        public Generic(KeyValuePair<SubscribableEvents, ChainOfCommandLevels> pair)
        {
            Event = pair.Key;
            Level = pair.Value;
        }
    }
}