using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Rules
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CanEditIfSelfAttribute : Attribute
    {
    }
}
