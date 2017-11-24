using System;

namespace CommandCentral.DTOs.EmailAddress
{
    public class Get : Post
    {
        public Guid Id { get; set; }

        public Get(Entities.EmailAddress item)
        {
            Id = item.Id;
            Address = item.Address;
            IsPreferred = item.IsPreferred;
            IsReleasableOutsideCoC = item.IsReleasableOutsideCoC;
            Person = item.Person.Id;
        }
    }
}
