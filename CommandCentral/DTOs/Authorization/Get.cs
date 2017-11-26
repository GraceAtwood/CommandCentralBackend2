using CommandCentral.Enums;
using System;
using System.Collections.Generic;

namespace CommandCentral.DTOs.Authorization
{
    public class Get
    {
        public List<string> PermissionGroupNames { get; set; }
        public Guid PersonId { get; set; }
        public Guid? PersonResolvedAgainstId { get; set; }
        public bool IsSelf { get; set; }
        public Dictionary<string, Dictionary<string, PropertyPermissionsDTO>> FieldPermissions { get; set; }
        public Dictionary<ChainOfCommandLevels, Dictionary<string, List<string>>> ReturnableFieldsAtLevel { get; set; }
        public Dictionary<ChainsOfCommand, ChainOfCommandLevels> HighestLevels { get; set; }
        public Dictionary<ChainsOfCommand, bool> IsInChainOfCommand { get; set; }
        public List<string> EditablePermissionGroups { get; set; }
        public List<SpecialPermissions> AccessibleSubmodules { get; set; }

        public class PropertyPermissionsDTO
        {
            public bool CanEdit { get; set; }
            public bool CanReturn { get; set; }
        }
    }
}
