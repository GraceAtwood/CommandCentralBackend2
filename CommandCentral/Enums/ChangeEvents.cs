using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Enums
{
    public enum ChangeEvents
    {
        MusterFinalized,
        MusterOpened,
        MusterReopened,
        MsuterEntryDeleted,
        MusterEntrySubmitted,
        LoginFailed,
        PersonCreated,
        CorrespondenceCreated,
        NewReviewSubmitted,
        ReviewStatusChanged,
        CorrespondenceRoutedToNextPerson,
        CorrespondenceCompleted,
        CorrespondenceShared,
        CorrespondenceDeleted,
        CorrespondenceModified
    }
}
