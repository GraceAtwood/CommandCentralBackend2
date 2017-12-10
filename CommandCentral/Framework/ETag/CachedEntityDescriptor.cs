using System;

namespace CommandCentral.Framework.ETag
{
    public class CachedEntityDescriptor
    {
        public string ETag { get; set; }

        public object Entity { get; set; }

        public Guid EntityId { get; set; }

        public DateTime DateTime { get; set; }
    }
}