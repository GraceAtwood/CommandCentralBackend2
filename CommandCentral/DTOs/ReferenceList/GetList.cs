using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandCentral.DTOs.ReferenceList
{
    public class GetList
    {
        public string Type { get; set; }
        public bool IsEditable { get; set; }
        public List<Get> Values { get; set; } = new List<Get>();

        public GetList(IEnumerable<ReferenceListItemBase> items)
        {
            if (items.Any())
            {
                var type = items.First().GetType();
                Type = type.Name;
                IsEditable = type.GetCustomAttribute<EditableReferenceListAttribute>() != null;

                foreach (var item in items)
                {
                    if (item.GetType() != type)
                        throw new ArgumentException("All items must be of the same type.", nameof(items));

                    Values.Add(new Get(item));
                }
            }
        }
    }
}
