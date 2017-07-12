using CommandCentral.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Rules
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HasPermissionsAttribute : Attribute
    {
    }
}
