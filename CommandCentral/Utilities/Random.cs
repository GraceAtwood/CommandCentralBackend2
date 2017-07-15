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
            string result = "";
            for (int x = 0; x < 10; x++)
            {
                result += GetRandomNumber(1, 9).ToString();
            }
            return result;
        }

        public static string GenerateSSN(string delimiter = "")
        {
            int iThree = GetRandomNumber(132, 921);
            int iTwo = GetRandomNumber(12, 83);
            int iFour = GetRandomNumber(1423, 9211);
            return iThree.ToString() + delimiter + iTwo.ToString() + delimiter + iFour.ToString();
        }

        public static int GetRandomNumber(int min, int max)
        {
            return Instance.Next(min, max);
        }
    }
}
