using System.Collections.Generic;
using System.Linq;

namespace CommandCentral.Utilities
{
    public static class CollectionUtilities
    {

        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (var s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (var s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        /// <summary>
        /// Does a shuffle using the Fisher-Yates shuffle algorithm.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var list = source.ToList();

            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Random.Instance.Next(n + 1);

                var t = list[k];
                list[k] = list[n];
                list[n] = t;
            }

            return list;
        }
    }
}
