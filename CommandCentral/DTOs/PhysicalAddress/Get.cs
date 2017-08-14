using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.PhysicalAddress
{
    public class Get : Update
    {
        public Guid Id { get; set; }
        public Guid Person { get; set; }

        public Get(Entities.PhysicalAddress item)
        {
            Id = item.Id;
            Person = item.Person.Id;
            Address = item.Address;
            City = item.City;
            Country = item.Country;
            IsHomeAddress = item.IsHomeAddress;
            IsReleaseableOutsideCoC = item.IsReleasableOutsideCoC;
            State = item.State;
            ZipCode = item.ZipCode;
        }
    }
}
