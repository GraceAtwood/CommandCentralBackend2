using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommandCentral.DTOs.ReferenceList
{
    public class GetList
    {
        public string Type { get; set; }
        public bool IsEditable { get; set; }
        public List<Get> Values { get; set; } = new List<Get>();

        public GetList(IEnumerable<ReferenceListItemBase> items, MemberInfo listType)
        {
            Type = listType.Name;
            IsEditable = listType.GetCustomAttribute<EditableReferenceListAttribute>() != null;
            
            if (!items.Any()) 
                return;

            foreach (var item in items)
            {
                Values.Add(new Get(item));
            }
        }
    }
}
