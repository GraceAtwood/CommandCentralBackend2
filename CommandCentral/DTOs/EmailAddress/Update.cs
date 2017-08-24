namespace CommandCentral.DTOs.EmailAddress
{
    public class Update
    {
        public bool IsReleasableOutsideCoC { get; set; }
        public string Address { get; set; }
        public bool IsPreferred { get; set; }
    }
}
