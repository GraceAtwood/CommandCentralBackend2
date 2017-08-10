using CommandCentral.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Events.Args
{
    public class PersonCreatedEventArgs
    {
        public Person Person { get; set; }
        public Person CreatedBy { get; set; }
    }
}
