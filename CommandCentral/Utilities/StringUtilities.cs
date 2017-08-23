using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Utilities
{
    public static class StringUtilities
    {

        /// <summary>
        /// Performs a case insensitive, current culture string comparison.  Handles nulls.
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool InsensitiveEquals(this string str1, string str)
        {
            return str != null && str1.Equals(str, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool ContainsInsensitive(this string str, string other, CultureInfo culture)
        {
            return culture.CompareInfo.IndexOf(str, other, CompareOptions.IgnoreCase) >= 0;
        }

        public static void ToConsole(this object obj)
        {
            Console.WriteLine(obj);
        }

        public static void ToConsole(this string str, params object[] args)
        {
            Console.WriteLine(str, args);
        }

        /// <summary>
        /// Null safe null check for string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool NullSafeIsNullOrWhitespace(this string str)
        {
            return String.IsNullOrWhiteSpace(str);
        }

        public static IEnumerable<string> SplitByAnd(this string str)
        {
            return str.Split('+').Select(x => x.Trim());
        }

        public static IEnumerable<string> SplitByOr(this string str)
        {
            return str.Split('|').Select(x => x.Trim());
        }

    }
}
