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

        public static string Add(Entity entity, object messageBody, DateTime dateTime)
        {
            var etag = Utilities.Random.CreateCryptographicallySecureGuid().ToString();

            var desc = new CachedEntityDescriptor
            {
                DateTime = dateTime,
                Entity = messageBody,
                EntityId = entity.Id,
                ETag = etag
            };
            
            if (!CachedEntityDescriptors.TryAdd(etag, desc))
                throw new Exception("Failed to add etag...");

            return etag;
        }
    }
}