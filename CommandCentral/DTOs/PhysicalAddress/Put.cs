namespace CommandCentral.DTOs.PhysicalAddress
{
    public class Put
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public bool IsHomeAddress { get; set; }
        public bool IsReleaseableOutsideCoC { get; set; }
    }
}
