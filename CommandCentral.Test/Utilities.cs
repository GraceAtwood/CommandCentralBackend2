using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Test
{
    public static class Utilities
    {
        public static string GenerateDoDId()
        {
            string result = "";
            for (int x = 0; x < 10; x++)
            {
                result += GetRandomNumber(1, 9).ToString();
            }
            return result;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
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
            return random.Next(min, max);
        }
    }
}
