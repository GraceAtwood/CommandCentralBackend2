using CommandCentral.Enums;

namespace CommandCentral.DTOs.WatchShiftType
{
    public class Put
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public WatchQualifications Qualification { get; set; }
    }
}