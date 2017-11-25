using System;

namespace CommandCentral.DTOs.PhysicalAddress
{
    public class Get : Post
    {
        public Guid Id { get; set; }

        public Get(Entities.PhysicalAddress item)
        {
            Id = item.Id;
            Person = item.Person.Id;
            Address = item.Address;
            City = item.City;
            Country = item.Country;
            IsHomeAddress = item.IsHomeAddress;
            IsReleasableOutsideCoC = item.IsReleasableOutsideCoC;
            State = item.State;
            ZipCode = item.ZipCode;
        }
    }
}
