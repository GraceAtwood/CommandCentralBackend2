using System;
using CommandCentral.Enums;

namespace CommandCentral.Authorization.Rules
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CanEditIfInChainOfCommandAttribute : Attribute
    {

        public ChainsOfCommand ChainOfCommand { get; private set; }
        public ChainOfCommandLevels Level { get; private set; }

        public CanEditIfInChainOfCommandAttribute(ChainsOfCommand coc, ChainOfCommandLevels level)
        {
            ChainOfCommand = coc;
            Level = level;
        }

    }
}
