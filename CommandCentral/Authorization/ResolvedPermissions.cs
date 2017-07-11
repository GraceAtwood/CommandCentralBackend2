using CommandCentral.Entities;
using CommandCentral.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Defines a class that stores a permission group after it has been refined.
    /// </summary>
    public class ResolvedPermissions
    {
        /// <summary>
        /// The list of permission groups' names that went into this resolved permission.
        /// </summary>
        public HashSet<PermissionGroup> PermissionGroups { get; set; } = new HashSet<PermissionGroup>();
        
        /// <summary>
        /// The client for whom this resolved permission was made.
        /// </summary>
        public Person Client { get; set; }

        /// <summary>
        /// The person against which these permissions were resolved.
        /// </summary>
        public Person PersonResolvedAgainst { get; set; }

        /// <summary>
        /// The list of editable fields, broken down by what type they belong to.  The key is case insensitive.
        /// </summary>
        public Dictionary<Type, HashSet<string>> EditableFields { get; set; } 
            = new Dictionary<Type, HashSet<string>>();

        /// <summary>
        /// The list of returnable fields, broken down by what module and type they belong to.  The key is case insensitive.
        /// </summary>
        public Dictionary<Type, HashSet<string>> ReturnableFields { get; set; }
            = new Dictionary<Type, HashSet<string>>();

        /// <summary>
        /// The list of fields that the client can return, with stipulations.
        /// </summary>
        public Dictionary<ChainOfCommandLevels, Dictionary<Type, HashSet<string>>> ReturnableFieldsAtLevel { get; set; } = 
            ((ChainOfCommandLevels[])Enum.GetValues(typeof(ChainOfCommandLevels))).ToDictionary(x => x, x => new Dictionary<Type, HashSet<string>>());

        /// <summary>
        /// The highest levels in each of the chains of command that this client has.  The key is case insensitive.
        /// </summary>
        public Dictionary<ChainsOfCommand, ChainOfCommandLevels> HighestLevels { get; set; } = ((ChainsOfCommand[])Enum.GetValues(typeof(ChainsOfCommand))).ToDictionary(x => x, x => ChainOfCommandLevels.None);

        /// <summary>
        /// A dictionary where the key is a chain of command and the boolean indicates if the client is in that chain of command for the given person.
        /// <para/>
        /// Guaranteed to contain all chains of command.
        /// </summary>
        public Dictionary<ChainsOfCommand, bool> IsInChainOfCommand { get; set; } = ((ChainsOfCommand[])Enum.GetValues(typeof(ChainsOfCommand))).ToDictionary(x => x, x => false);

        /// <summary>
        /// The list of those permission groups' names that this client can edit the membership of.
        /// </summary>
        public HashSet<PermissionGroup> EditablePermissionGroups { get; set; } = new HashSet<PermissionGroup>();

        /// <summary>
        /// The list of all submodules this client can access.
        /// </summary>
        public HashSet<SubModules> AccessibleSubmodules { get; set; } = new HashSet<SubModules>();

        public ResolvedPermissions(Person person, Person personResolvedAgainst)
        {
            foreach (var group in person.PermissionGroups)
            {

            }
        }

    }
}
