using System;

namespace CommandCentral.Authorization.Rules
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HasPermissionsAttribute : Attribute
    {
    }
}
