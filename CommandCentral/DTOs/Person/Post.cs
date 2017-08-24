using System;

namespace CommandCentral.DTOs.Person
{
    public class Post
    {
        public Guid Paygrade { get; set; }
        public Guid Designation { get; set; }
        public Guid UIC { get; set; }
        public Guid DutyStatus { get; set; }
        public Guid Sex { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string SSN { get; set; }
        public string DoDId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime DateOfArrival { get; set; }
        public Guid Division { get; set; }
    }
}
