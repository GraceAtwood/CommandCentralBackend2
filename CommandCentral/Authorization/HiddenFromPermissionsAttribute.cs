using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HiddenFromPermissionsAttribute : Attribute
    {
    }
}
