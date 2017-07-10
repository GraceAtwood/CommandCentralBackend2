using System;
using System.Collections.Generic;
using System.Linq;
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
            if (obj == null)
                return 0;

            return obj.GetHashCode();
        }

    }
}
