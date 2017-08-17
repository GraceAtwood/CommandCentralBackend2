using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandCentral.Utilities;

namespace CommandCentral.DTOs.ReferenceList
{
    public class GetList
    {
        public string Type { get; set; }
        public bool IsEditable { get; set; }
        public List<Get> Values { get; set; } = new List<Get>();

        public GetList(IEnumerable<ReferenceListItemBase> items, Type listType)
        {
            if (items.Any())
            {
                Type = listType.Name;
                IsEditable = listType.GetCustomAttribute<EditableReferenceListAttribute>() != null;

                foreach (var item in items)
                {
                    Values.Add(new Get(item));
                }
            }
        }
    }
}
