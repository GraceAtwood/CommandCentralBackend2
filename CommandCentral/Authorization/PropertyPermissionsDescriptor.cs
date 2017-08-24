using System.Reflection;

namespace CommandCentral.Authorization
{
    public class PropertyPermissionsDescriptor
    {
        public PropertyInfo Property { get; set; }
        public bool CanEdit { get; set; }
        public bool CanReturn { get; set; }
    }
}
