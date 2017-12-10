using System;
using System.Collections.Concurrent;

namespace CommandCentral.Framework.ETag
{
    public static class ETagCache
    {
        private static ConcurrentDictionary<string, CachedEntityDescriptor> CachedEntityDescriptors { get; } =
            new ConcurrentDictionary<string, CachedEntityDescriptor>();

        public static bool TryGetCachedEntityDescriptor(string eTag, out CachedEntityDescriptor cachedEntityDescriptor)
        {
            return CachedEntityDescriptors.TryGetValue(eTag, out cachedEntityDescriptor);
        }
    }
}