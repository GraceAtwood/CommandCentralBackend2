using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Utilities
{
    public static class CollectionUtilities
    {

        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
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
            foreach (T s in list2)
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
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var list = source.ToList();
            var random = new Random(DateTime.Now.Millisecond);

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);

                T t = list[k];
                list[k] = list[n];
                list[n] = t;
            }

            return list;
        }

    }
}
