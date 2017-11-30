using System;

namespace CommandCentral.DTOs.Room
{
    public class Post : Put
    {
        public int Number { get; set; }
        public int Level { get; set; }
        public Guid Building { get; set; }
    }
}