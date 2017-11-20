using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandCentral.DTOs.Command
{
    public class Get : Update
    {
        public Guid Id { get; set; }
        public List<Guid> Departments { get; set; }
        public Guid CurrentMusterCycle { get; set; }

        public Get(Entities.Command item)
        {
            Id = item.Id;
            Name = item.Name;
            Description = item.Description;
            Departments = item.Departments.Select(x => x.Id).ToList();
            Address = item.Address;
            City = item.City;
            Country = item.Country;
            MusterStartHour = item.MusterStartHour;
            State = item.State;
            ZipCode = item.ZipCode;
            CurrentMusterCycle = item.CurrentMusterCycle.Id;
            TimeZoneId = item.TimeZoneId;
        }
    }
}
