using System;

namespace CommandCentral.Authorization.Rules
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HiddenFromPermissionsAttribute : Attribute
    {
    }
}
