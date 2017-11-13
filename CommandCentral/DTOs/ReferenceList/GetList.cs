using CommandCentral.Entities.ReferenceLists;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommandCentral.DTOs.ReferenceList
{
    public class GetList
    {
        public string Type { get; set; }
        public List<Get> Values { get; set; } = new List<Get>();

        public GetList(IEnumerable<ReferenceListItemBase> items, MemberInfo listType)
        {
            Type = listType.Name;
            
            if (!items.Any()) 
                return;

            foreach (var item in items)
            {
                Values.Add(new Get(item));
            }
        }
    }
}
