namespace CommandCentral.DTOs.Command
{
    public class Update
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public int MusterStartHour { get; set; }
        public string TimeZoneId { get; set; }
    }
}
