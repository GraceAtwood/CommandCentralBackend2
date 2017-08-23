using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandCentral.Utilities
{
    public static class NullSafeUtilities
    {

        /// <summary>
        /// A null safe method for getting an object's hashcode.  Returns 0 if the object is null.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int GetSafeHashCode(object obj)
        {
            return obj == null 
                ? 0 
                : obj.GetHashCode();
        }

        /// <summary>
        /// Returns the property infos for all properties that are not set to null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetNonDefaultOrNullProperties<T>(this T obj)
        {
            return obj == null 
                ? new List<PropertyInfo>() 
                : typeof(T).GetProperties().Where(x => x.GetValue(obj) != null);
        }
    }
}
