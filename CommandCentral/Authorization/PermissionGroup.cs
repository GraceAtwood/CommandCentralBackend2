using CommandCentral.Enums;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    public class PermissionGroup
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsMemberOfChainOfCommand { get; set; }

        public Dictionary<ChainsOfCommand, ChainOfCommandLevels> AccessLevels { get; set; }

        public HashSet<SubModules> AccessibleSubmodules { get; set; }

        public HashSet<string> EditablePermissionGroups { get; set; }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is PermissionGroup group && group.Name == Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
