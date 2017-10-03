using System;
using System.Linq;
using System.Security.Cryptography;

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
        
        /// <summary>
        /// Returns a cryptograhically secure Guid.
        /// </summary>
        /// <returns></returns>
        public static Guid CreateCryptographicallySecureGuid() 
        {
            using (var provider = RandomNumberGenerator.Create()) 
            {
                var bytes = new byte[16];
                provider.GetBytes(bytes);

                return new Guid(bytes);
            }
        }

        /// <summary>
        /// Randomly selects an enum value from the given enumeration.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TEnum GetRandomEnumValue<TEnum>()
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("The given type was not an enum.", nameof(TEnum));

            return ((TEnum[]) Enum.GetValues(typeof(TEnum))).Shuffle().First();
        }
    }
}
