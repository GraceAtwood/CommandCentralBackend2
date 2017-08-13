using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.ReferenceList
{
    public class Get : Post
    {
        public Guid Id { get; set; }

        public Get(Entities.ReferenceLists.ReferenceListItemBase item)
        {
            Id = item.Id;
            Value = item.Value;
            Description = item.Description;
            Type = item.GetType().Name;
        }
    }
}
