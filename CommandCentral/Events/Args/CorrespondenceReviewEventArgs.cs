using System;
using CommandCentral.Entities.Correspondence;

namespace CommandCentral.Events.Args
{
    public class CorrespondenceReviewEventArgs : EventArgs
    {
        public CorrespondenceReview Review { get; set; }
    }
}
