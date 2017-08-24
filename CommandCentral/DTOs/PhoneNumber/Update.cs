using System;

namespace CommandCentral.DTOs.PhoneNumber
{
    public class Update
    { 
        public Guid PhoneType { get; set; }
        public bool IsReleasableOutsideCoC { get; set; }
        public string Number { get; set; }
        public bool IsPreferred { get; set; }
    }
}
