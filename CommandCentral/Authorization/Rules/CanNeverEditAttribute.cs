﻿using System;

namespace CommandCentral.Authorization.Rules
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CanNeverEditAttribute : Attribute
    {
    }
}
