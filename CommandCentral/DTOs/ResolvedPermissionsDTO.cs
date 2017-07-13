using CommandCentral.Authorization;
using CommandCentral.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.DTOs
{
    public class ResolvedPermissionsDTO
    {

        public List<string> PermissionGroupNames { get; set; }
        public Guid PersonId { get; set; }
        public Guid PersonResolvedAgainstId { get; set; }
        public bool IsSelf { get; set; }
        public Dictionary<string, Dictionary<string, PropertyPermissionsDescriptor>> FieldPermissions { get; set; }
        public Dictionary<ChainOfCommandLevels, Dictionary<string, List<string>>> ReturnableFieldsAtLevel { get; set; }
        public Dictionary<ChainsOfCommand, ChainOfCommandLevels> HighestLevels { get; set; }
        public Dictionary<ChainsOfCommand, bool> IsInChainOfCommand { get; set; }
        public List<string> EditablePermissionGroups { get; set; }
        public List<SubModules> AccessibleSubmodules { get; set; }

    }
}
