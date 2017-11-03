using CommandCentral.Enums;

namespace CommandCentral.DTOs.Watchbill
{
    public class Post
    {
        public string Title { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public Entities.Command Command { get; set; }
    }
}