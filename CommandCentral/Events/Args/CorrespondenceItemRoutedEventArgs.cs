using System;
using CommandCentral.Entities;
using CommandCentral.Entities.Correspondence;

namespace CommandCentral.Events.Args
{
    public class CorrespondenceItemRoutedEventArgs : EventArgs
    {
        public CorrespondenceItem Item { get; set; }

        public Person NewPersonRoutedTo { get; set; }
    }
}
