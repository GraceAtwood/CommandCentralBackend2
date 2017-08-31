using CommandCentral.Enums;

namespace CommandCentral.DTOs.SubscribedEvents
{
    public class Update
    {
        public SubscribableEvents Event { get; set; }
        public ChainOfCommandLevels Level { get; set; }
    }
}