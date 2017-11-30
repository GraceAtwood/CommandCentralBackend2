using CommandCentral.Enums;

namespace CommandCentral.DTOs.Watchbill
{
    public class Put
    {
        public string Title { get; set; }
        public WatchbillPhases Phase { get; set; }
    }
}