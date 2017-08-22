using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.NewsItem
{
    public class Get : Update
    {
        public Guid Id { get; set; }
        public Guid Creator { get; set; }
        public DateTime? CreationTime { get; set; }

        public Get(Entities.NewsItem item)
        {
            Id = item.Id;
            Body = item.Body;
            Title = item.Title;
            CreationTime = item.CreationTime;
            Creator = item.Creator.Id;
        }
    }
}
