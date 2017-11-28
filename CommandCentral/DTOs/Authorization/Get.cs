using CommandCentral.Enums;
using System;
using System.Collections.Generic;

namespace CommandCentral.DTOs.Authorization
{
    public class Get
    {
        public Guid PersonId { get; set; }
        public Guid? PersonResolvedAgainstId { get; set; }
        public Dictionary<ChainsOfCommand, ChainOfCommandLevels> HighestLevels { get; set; }
        public Dictionary<ChainsOfCommand, bool> IsInChainOfCommand { get; set; }
        public List<SpecialPermissions> AccessibleSubmodules { get; set; }
    }
}
