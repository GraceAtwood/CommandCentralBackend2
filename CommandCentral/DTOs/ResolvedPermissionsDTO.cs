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

        public List<string> PermissionGroupNames { get; private set; }
        public Guid PersonId { get; private set; }
        public Guid PersonResolvedAgainst { get; private set; }
        public bool IsSelf { get; private set; }
        public Dictionary<string, Dictionary<string, PropertyPermissionsDescriptor>> FieldPermissions { get; private set; }
        public Dictionary<ChainOfCommandLevels, Dictionary<string, HashSet<string>>> ReturnableFieldsAtLevel { get; private set; }
        public Dictionary<ChainsOfCommand, ChainOfCommandLevels> HighestLevels { get; private set; }
        public Dictionary<ChainsOfCommand, bool> IsInChainOfCommand { get; private set; }
        public HashSet<string> EditablePermissionGroups { get; private set; }
        public HashSet<SubModules> AccessibleSubmodules { get; private set; }

    }
}
