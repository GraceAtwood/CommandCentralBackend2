using System;
using Microsoft.AspNetCore.Razor.Language;

namespace CommandCentral.DTOs.WatchShift
{
    public class Get : Post
    {
        public Guid Id { get; set; }
        public Guid? WatchAssignment { get; set; }
        public Guid? DivisionAssignedTo { get; set; }

        public Get(Entities.Watchbill.WatchShift item)
        {
            Id = item.Id;
            Title = item.Title;
            Range = item.Range;
            ShiftType = item.ShiftType.Id;
            Watchbill = item.Watchbill.Id;
            WatchAssignment = item.WatchAssignment?.Id;
            DivisionAssignedTo = item.DivisionAssignedTo?.Id;
        }
    }
}