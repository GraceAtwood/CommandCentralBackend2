using CommandCentral.Entities;
using CommandCentral.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public bool IsSelf
        {
            get
            {
                return Client.Id == PersonResolvedAgainst.Id;
            }
        }

        /// <summary>
        /// The list of editable fields, broken down by what type they belong to.  The key is case insensitive.
        /// </summary>
        public Dictionary<Type, Dictionary<string, PropertyPermissionsDescriptor>> FieldPermissions
            = new Dictionary<Type, Dictionary<string, PropertyPermissionsDescriptor>>();

        /// <summary>
        /// The list of fields that the client can return, with stipulations.
        /// </summary>
        public Dictionary<ChainOfCommandLevels, Dictionary<Type, HashSet<PropertyInfo>>> ReturnableFieldsAtLevel { get; set; } = 
            ((ChainOfCommandLevels[])Enum.GetValues(typeof(ChainOfCommandLevels))).ToDictionary(x => x, x => new Dictionary<Type, HashSet<PropertyInfo>>());

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
            Client = person;
            PersonResolvedAgainst = personResolvedAgainst;

            PermissionGroups = new HashSet<PermissionGroup>(person.PermissionGroups);

            foreach (var group in PermissionGroups)
            {
                foreach (var accessLevel in group.AccessLevels)
                {
                    if (accessLevel.Value > HighestLevels[accessLevel.Key])
                        HighestLevels[accessLevel.Key] = accessLevel.Value;
                }

                foreach (var editablePermissionGroupName in group.EditablePermissionGroups)
                {
                    EditablePermissionGroups.Add(PermissionsCache.PermissionGroupsCache[editablePermissionGroupName]);
                }

                foreach (var subModule in group.AccessibleSubmodules)
                {
                    AccessibleSubmodules.Add(subModule);
                }
            }

            foreach (var pair in PermissionsCache.PermissionTypesCache)
            {
                var permissions = new Dictionary<string, PropertyPermissionsDescriptor>();
                
                foreach (var property in pair.Value)
                {

                    if (!property.HiddenFromPermission)
                    {
                        var descriptor = new PropertyPermissionsDescriptor();
                        
                        if (property.CanEditIfSelf && IsSelf)
                        {
                            descriptor.CanEdit = true;
                        }
                        else
                        {
                            foreach (var accessLevel in property.LevelsRequiredToEditForChainOfCommand)
                            {
                                //The property will start off as false.  If it ever gets set to true, then break.
                                if (descriptor.CanEdit)
                                    break;

                                if (HighestLevels[accessLevel.Key] >= accessLevel.Value)
                                {
                                    //Here we know that the person has an access level that is high enough.
                                    //Now we need to make sure they're in the same div, dep, or command.
                                    switch (accessLevel.Value)
                                    {
                                        case ChainOfCommandLevels.Command:
                                            {
                                                if (person.IsInSameCommandAs(personResolvedAgainst));
                                                    descriptor.CanEdit = true;

                                                break;
                                            }
                                        case ChainOfCommandLevels.Department:
                                            {
                                                if (person.IsInSameDepartmentAs(personResolvedAgainst) ||
                                                    person.IsInSameCommandAs(personResolvedAgainst))
                                                    descriptor.CanEdit = true;

                                                break;
                                            }
                                        case ChainOfCommandLevels.Division:
                                            {
                                                if (person.IsInSameDivisionAs(personResolvedAgainst) ||
                                                    person.IsInSameDepartmentAs(personResolvedAgainst) ||
                                                    person.IsInSameCommandAs(PersonResolvedAgainst))
                                                    descriptor.CanEdit = true;

                                                break;
                                            }
                                        default:
                                            {
                                                throw new NotImplementedException();
                                            }
                                    }
                                }
                            }
                        }

                        if (property.CanReturnIfSelf && IsSelf)
                        {
                            descriptor.CanReturn = true;
                        }
                        else
                        {
                            foreach (var accessLevel in property.LevelsRequiredToReturnForChainOfCommand)
                            {
                                //The property will start off as false.  If it ever gets set to true, then break.
                                if (descriptor.CanReturn)
                                    break;

                                if (HighestLevels[accessLevel.Key] >= accessLevel.Value)
                                {
                                    //Here we know that the person has an access level that is high enough.
                                    //Now we need to make sure they're in the same div, dep, or command.
                                    switch (accessLevel.Value)
                                    {
                                        case ChainOfCommandLevels.Command:
                                            {
                                                if (person.IsInSameCommandAs(personResolvedAgainst));
                                                descriptor.CanReturn = true;

                                                break;
                                            }
                                        case ChainOfCommandLevels.Department:
                                            {
                                                if (person.IsInSameDepartmentAs(personResolvedAgainst) ||
                                                    person.IsInSameCommandAs(personResolvedAgainst))
                                                    descriptor.CanReturn = true;

                                                break;
                                            }
                                        case ChainOfCommandLevels.Division:
                                            {
                                                if (person.IsInSameDivisionAs(personResolvedAgainst) ||
                                                    person.IsInSameDepartmentAs(personResolvedAgainst) ||
                                                    person.IsInSameCommandAs(PersonResolvedAgainst))
                                                    descriptor.CanReturn = true;

                                                break;
                                            }
                                        default:
                                            {
                                                throw new NotImplementedException();
                                            }
                                    }
                                }
                            }
                        }

                        permissions[property.Property.Name] = descriptor;
                    }
                }

                FieldPermissions[pair.Key] = permissions;
            }
        }

    }
}
