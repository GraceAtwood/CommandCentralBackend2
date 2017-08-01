using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.NewsItem
{
    public class Get : Update
    {
        public Guid Id { get; set; }
        public Guid? Creator { get; set; }
        public DateTime? CreationTime { get; set; }
    }
}
