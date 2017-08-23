using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Utilities
{
    public static class Random
    {
        public static System.Random Instance = new System.Random(DateTime.Now.Millisecond);

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Instance.Next(s.Length)]).ToArray());
        }

        public static string GenerateDoDId()
        {
            var result = "";
            for (var x = 0; x < 10; x++)
            {
                result += GetRandomNumber(1, 9).ToString();
            }
            return result;
        }

        public static string GenerateSSN(string delimiter = "")
        {
            var iThree = GetRandomNumber(132, 921);
            var iTwo = GetRandomNumber(12, 83);
            var iFour = GetRandomNumber(1423, 9211);
            return iThree + delimiter + iTwo + delimiter + iFour;
        }

        public static int GetRandomNumber(int min, int max)
        {
            return Instance.Next(min, max);
        }
    }
}
