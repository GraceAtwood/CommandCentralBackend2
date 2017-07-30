using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    public class PropertyPermissionsDescriptor
    {
        public PropertyInfo Property { get; set; }
        public bool CanEdit { get; set; }
        public bool CanReturn { get; set; }
    }
}
