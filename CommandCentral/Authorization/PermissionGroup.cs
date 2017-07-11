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
        public virtual string Name { get; set; }

        public virtual bool IsMemberOfChainOfCommand { get; set; }

        public virtual Dictionary<ChainsOfCommand, ChainOfCommandLevels> AccessLevels { get; set; } 

        public virtual HashSet<SubModules> AccessibleSubmodules { get; set; }

        public virtual HashSet<Guid> EditablePermissionGroups { get; set; }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is PermissionGroup group && group.Name == this.Name;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
