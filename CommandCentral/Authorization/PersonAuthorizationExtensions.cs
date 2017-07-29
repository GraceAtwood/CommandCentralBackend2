using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    public static class PersonAuthorizationExtensions
    {
        public static Dictionary<ChainsOfCommand, ChainOfCommandLevels> GetHighestAccessLevels(this Person person)
        {
            var highestLevels = ((ChainsOfCommand[])Enum.GetValues(typeof(ChainsOfCommand))).ToDictionary(x => x, x => ChainOfCommandLevels.None);

            foreach (var accessLevel in person.PermissionGroups.SelectMany(x => x.AccessLevels))
            {
                if (highestLevels[accessLevel.Key] < accessLevel.Value)
                    highestLevels[accessLevel.Key] = accessLevel.Value;
            }

            return highestLevels;
        }

        /// <summary>
        /// Returns true or false if this person is in ANY given chain of command of the given person.  (Is the principle person in the chain of command of the "other" person)
        /// </summary>
        /// <param name="person">The principle person to check.</param>
        /// <param name="other">The person we are asking about.</param>
        /// <param name="chainsOfCommand">The list of chains of command to check.  If none are given, then we'll check if the principle person is in any chain of command for the other person at all.</param>
        /// <returns></returns>
        public static bool IsInChainOfCommand(this Person person, Person other, params ChainsOfCommand[] chainsOfCommand)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            HashSet<ChainsOfCommand> chainsToCheck = new HashSet<ChainsOfCommand>(chainsOfCommand);
            if (!chainsToCheck.Any())
            {
                foreach (var chain in (((ChainsOfCommand[])Enum.GetValues(typeof(ChainsOfCommand)))))
                    chainsToCheck.Add(chain);
            }

            if (person.PermissionGroups == null)
            {
                throw new ArgumentException("The given person's permission groups were null.", $"{person}.{nameof(person.PermissionGroups)}");
            }

            var highestLevels = GetHighestAccessLevels(person);

            foreach (var level in highestLevels)
            {
                switch (level.Value)
                {
                    case ChainOfCommandLevels.Command:
                        if (person.IsInSameCommandAs(other))
                            return true;
                        break;
                    case ChainOfCommandLevels.Department:
                        if (person.IsInSameDepartmentAs(other))
                            return true;
                        break;
                    case ChainOfCommandLevels.Division:
                        if (person.IsInSameDivisionAs(other))
                            return true;
                        break;
                    case ChainOfCommandLevels.None:
                        break;
                    default:
                        {
                            throw new NotImplementedException($"Fell to default in {nameof(IsInChainOfCommand)} switch. Value: {level.Value}");
                        }
                }
            }

            return false;
        }


        /// <summary>
        /// Retuns true or false indicating if this person can edit the membership of all of the given permission groups.
        /// </summary>
        /// <param name="person">The person whose permissions we want to check.</param>
        /// <param name="groups">The groups to check if this person can edit.</param>
        /// <returns></returns>
        public static bool CanEditPermissionGroups(this Person person, IEnumerable<PermissionGroup> groups)
        {
            if (groups == null || !groups.Any())
                throw new ArgumentException("You must give at least one group to check against.", nameof(groups));

            if (person.PermissionGroups == null)
            {
                throw new ArgumentException("The given person's permission groups were null.", $"{person}.{nameof(person.PermissionGroups)}");
            }

            var set = new HashSet<string>(person.PermissionGroups.SelectMany(x => x.EditablePermissionGroups));

            return groups.All(x => set.Contains(x.Name));
        }

        /// <summary>
        /// Returns true or false indicating if this person can access all of the given submodules.
        /// </summary>
        /// <param name="person">The person whose permissions we want to check.</param>
        /// <param name="submodules">The submodules to check if this person can access.</param>
        /// <returns></returns>
        public static bool CanAccessSubmodules(this Person person, params SubModules[] submodules)
        {
            return CanAccessSubmodules(person, subs: submodules);
        }

        /// <summary>
        /// Returns true or false indicating if this person can access all of the given submodules.
        /// </summary>
        /// <param name="person">The person whose permissions we want to check.</param>
        /// <param name="subs">The submodules to check if this person can access.</param>
        /// <returns></returns>
        public static bool CanAccessSubmodules(this Person person, IEnumerable<SubModules> subs)
        {
            if (subs == null || !subs.Any())
                throw new ArgumentException("You must give at least one group to check against.", nameof(subs));

            if (person.PermissionGroups == null)
            {
                throw new ArgumentException("The given person's permission groups were null.", $"{person}.{nameof(person.PermissionGroups)}");
            }

            var set = new HashSet<SubModules>(person.PermissionGroups.SelectMany(x => x.AccessibleSubmodules));

            return subs.All(set.Contains);
        }

        /// <summary>
        /// Returns the subset of properties that can be returned/selected by this person taken from the properties selected with "selectors".  
        /// If no properties are given, then all properties from the given type are considered.
        /// </summary>
        /// <typeparam name="T">The type that contains the properties in question.</typeparam>
        /// <param name="person">The person whose permissions we want to check.</param>
        /// <param name="other">The person to check permissions against.</param>
        /// <param name="selectors">A params array of selectors that provide the properties to check.</param>
        /// <returns></returns>
        public static TypePermissionsDescriptor<T> GetFieldPermissions<T>(this Person person, Person other)
        {
            return new TypePermissionsDescriptor<T>(person, other);
        }
    }
}
