using System;

namespace CommandCentral.DTOs.CFSRequest
{
    public class Get : Post
    {
        public DateTime TimeSubmitted { get; set; }
        public bool IsClaimed { get; set; }

        public Get(Entities.CFS.Request request)
        {
            TimeSubmitted = request.TimeSubmitted;
            IsClaimed = request.IsClaimed;
            Person = request.Person.Id;
            RequestType = request.RequestType;
            ClaimedBy = request.ClaimedBy.Id;
        }
    }
}