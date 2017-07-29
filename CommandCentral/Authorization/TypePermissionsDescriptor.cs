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
    public class TypePermissionsDescriptor<T>
    {
        public Person Person { get; private set; }
        public Person Other { get; private set; }

        public Dictionary<ChainsOfCommand, ChainOfCommandLevels> PersonHighestLevels { get; private set; }

        public TypePermissionsDescriptor(Person person, Person other)
        {
            this.Person = person;
            this.Other = other;

            this.PersonHighestLevels = person.GetHighestAccessLevels();
        }

        public bool CanReturn<TValue>(Expression<Func<T, TValue>> selector)
        {
            var permissions = PermissionsCache.PermissionTypesCache[typeof(T)][(PropertyInfo)selector.GetProperty()];

            if (permissions.CanReturnIfSelf && Other != null && Person.Equals(Other))
            {
                return true;
            }

            if (permissions.LevelsRequiredToReturnForChainOfCommand.All(level =>
            {
                if (level.Value != ChainOfCommandLevels.None && Other == null)
                    return false;

                if (PersonHighestLevels[level.Key] < level.Value)
                    return false;

                switch (level.Value)
                {
                    case ChainOfCommandLevels.Command:
                        if (!Person.IsInSameCommandAs(Other))
                            return false;
                        break;
                    case ChainOfCommandLevels.Department:
                        if (!Person.IsInSameDepartmentAs(Other))
                            return false;
                        break;
                    case ChainOfCommandLevels.Division:
                        if (!Person.IsInSameDivisionAs(Other))
                            return false;
                        break;
                    case ChainOfCommandLevels.None:
                        break;
                    default:
                        throw new NotImplementedException($"Fell to default of switch in {nameof(CanReturn)}.  Value: {level.Value}");
                }

                return true;
            }))
            {
                return true;
            }

            return false;
        }

        public TValue GetSafeReturnValue<TValue>(T obj, Expression<Func<T, TValue>> selector)
        {
            if (CanReturn(selector))
                return selector.Compile()(obj);
            else
                return default(TValue);
        }

        public bool CanEdit<TValue>(Expression<Func<T, TValue>> selector)
        {
            var permissions = PermissionsCache.PermissionTypesCache[typeof(T)][(PropertyInfo)selector.GetProperty()];

            if (permissions.CanNeverEdit)
                return false;

            if (permissions.CanEditIfSelf && Other != null && Person.Equals(Other))
            {
                return true;
            }

            if (permissions.LevelsRequiredToEditForChainOfCommand.All(level =>
            {
                if (level.Value != ChainOfCommandLevels.None && Other == null)
                    return false;

                if (PersonHighestLevels[level.Key] < level.Value)
                    return false;

                switch (level.Value)
                {
                    case ChainOfCommandLevels.Command:
                        if (!Person.IsInSameCommandAs(Other))
                            return false;
                        break;
                    case ChainOfCommandLevels.Department:
                        if (!Person.IsInSameDepartmentAs(Other))
                            return false;
                        break;
                    case ChainOfCommandLevels.Division:
                        if (!Person.IsInSameDivisionAs(Other))
                            return false;
                        break;
                    case ChainOfCommandLevels.None:
                        break;
                    default:
                        throw new NotImplementedException($"Fell to default of switch in {nameof(CanEdit)}.  Value: {level.Value}");
                }

                return true;
            }))
            {
                return true;
            }

            return false;
        }
    }
}
