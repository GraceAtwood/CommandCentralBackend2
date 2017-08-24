using Newtonsoft.Json.Serialization;
using System;

namespace CommandCentral.Framework
{
    /// <summary>
    /// Provides custom contract resolution.  This allows us to make broad changes to how serialization occurs across all objects.
    /// </summary>
    public class CustomContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            return base.CreateContract(typeof(NHibernate.Proxy.INHibernateProxy).IsAssignableFrom(objectType) 
                ? objectType.BaseType 
                : objectType);
        }
    }
}
