using System.Collections.Generic;
using System.Linq;

namespace CommandCentral.PreDefs
{
    public class PreDefOf<T> : IPreDef where T : class
    {
        public string TypeFullName => typeof(T).FullName;

        public List<T> Definitions { get; set; }

        public static PreDefOf<T> Get()
        {
            return (PreDefOf<T>)PreDefUtility.Predefs.FirstOrDefault(x => x.TypeFullName == typeof(T).FullName);
        }
    }
}
