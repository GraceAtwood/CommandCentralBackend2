using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandCentral.Utilities.Types
{
    public class CustomPropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
    {
        public bool Equals(PropertyInfo x, PropertyInfo y)
        {
            return Object.ReferenceEquals(x, y) || (x.Name == y.Name && x.DeclaringType == y.DeclaringType && x.PropertyType == y.PropertyType);
        }

        public int GetHashCode(PropertyInfo obj)
        {
            unchecked
            {
                int hash = 39;

                hash ^= 23 * obj.Name.GetHashCode();
                hash ^= 23 * obj.DeclaringType.GetHashCode();
                hash ^= 23 * obj.PropertyType.GetHashCode();

                return hash;
            }
        }
    }
}
