using CommandCentral.Enums;

namespace CommandCentral.DTOs.WatchShiftType
{
    public class Update
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public WatchQualifications Qualification { get; set; }
    }
}