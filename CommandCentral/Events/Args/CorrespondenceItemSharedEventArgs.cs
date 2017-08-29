using System;
using System.Collections.Generic;
using CommandCentral.Entities;
using CommandCentral.Entities.Correspondence;

namespace CommandCentral.Events.Args
{
    public class CorrespondenceItemSharedEventArgs : EventArgs
    {
        public CorrespondenceItem Item { get; set; }
        public IEnumerable<Person> NewPersons { get; set; }
    }
}
