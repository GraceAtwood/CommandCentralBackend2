using CommandCentral.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    public class CanEditAttribute : Attribute
    {

        public ChainsOfCommand ChainOfCommand { get; private set; }
        public ChainOfCommandLevels Level { get; private set; }

        public CanEditAttribute(ChainsOfCommand coc, ChainOfCommandLevels level)
        {
            ChainOfCommand = coc;
            Level = level;
        }

    }
}
