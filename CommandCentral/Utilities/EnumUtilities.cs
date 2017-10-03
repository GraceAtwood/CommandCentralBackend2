using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommandCentral.Utilities
{
    public static class EnumUtilities
    {
        private static readonly Dictionary<Type, Array> _enumsCache;
        
        static EnumUtilities()
        {
            _enumsCache = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsEnum)
                .ToDictionary(x => x, Enum.GetValues);
        }

        public static TEnum[] GetValues<TEnum>()
        {
            if (!_enumsCache.TryGetValue(typeof(TEnum), out Array array))
                throw new ArgumentException("Your given enumeration type was not found in the cache.", nameof(TEnum));

            return (TEnum[])array;
        }

        public static IEnumerable<TEnum> GetPartialValueMatches<TEnum>(string searchValue)
        {
            if (!_enumsCache.TryGetValue(typeof(TEnum), out Array array))
                throw new ArgumentException("Your given enumeration type was not found in the cache.", nameof(TEnum));

            return ((TEnum[]) array).Where(x => x.ToString().Contains(searchValue));
        }
    }
}