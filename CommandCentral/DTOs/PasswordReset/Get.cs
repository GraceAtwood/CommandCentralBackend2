using System;

namespace CommandCentral.DTOs.PasswordReset
{
    public class Get
    {
        public Guid Id { get; set; }
        public DateTime TimeSubmitted { get; set; }
        public Guid Person { get; set; }

        public Get(Entities.PasswordReset item)
        {
            Id = item.Id;
            TimeSubmitted = item.TimeSubmitted;
            Person = item.Person.Id;
        }
    }
}