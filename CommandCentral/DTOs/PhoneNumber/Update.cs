using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
