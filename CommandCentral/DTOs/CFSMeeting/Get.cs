using System;
using CommandCentral.Entities.CFS;

namespace CommandCentral.DTOs.CFSMeeting
{
    public class Get : Post
    {
        public Guid Id { get; set; }

        public Get(Meeting meeting)
        {
            Id = meeting.Id;
            Advisor = meeting.Advisor?.Id;
            Notes = meeting.Notes;
            Person = meeting.Person.Id;
            Range = meeting.Range;
            Request = meeting.Request.Id;
        }
    }
}