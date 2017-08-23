using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.Registration
{
    public class Put
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public Guid ConfirmationToken { get; set; }
    }
}