using System;

namespace CommandCentral.DTOs.PhoneNumber
{
    public class Get : Post
    {
        public Guid Id { get; set; }

        public Get(Entities.PhoneNumber item)
        {
            Id = item.Id;
            IsPreferred = item.IsPreferred;
            IsReleasableOutsideCoC = item.IsReleasableOutsideCoC;
            Number = item.Number;
            Person = item.Person.Id;
            PhoneType = item.PhoneType;
        }
    }
}
