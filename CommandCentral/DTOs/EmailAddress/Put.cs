namespace CommandCentral.DTOs.EmailAddress
{
    public class Put
    {
        public bool IsReleasableOutsideCoC { get; set; }
        public string Address { get; set; }
        public bool IsPreferred { get; set; }
    }
}
