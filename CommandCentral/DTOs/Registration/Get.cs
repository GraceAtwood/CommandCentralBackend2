using System;
using CommandCentral.Entities;

namespace CommandCentral.DTOs.Registration
{
    public class Get
    {
        public Guid Id { get; set; }
        public DateTime TimeSubmitted { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? TimeCompleted { get; set; }
        public Guid Person { get; set; }

        public Get(AccountRegistration item)
        {
            Id = item.Id;
            IsCompleted = item.IsCompleted;
            TimeCompleted = item.TimeCompleted;
            TimeSubmitted = item.TimeSubmitted;
            Person = item.Person.Id;
        }
    }
}