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

        private Dictionary<ChainsOfCommand, ChainOfCommandLevels> _personHighestLevels { get; set; }

        public TypePermissionsDescriptor(Person person, Person other)
        {
            this.Person = person;
            this.Other = other;

            this._personHighestLevels = person.GetHighestAccessLevels();
        }

        public bool CanReturn(string propertName)
        {
            if (!PermissionsCache.PermissionTypesCache.TryGetValue(typeof(T), out Dictionary<PropertyInfo, PropertyPermissionsCollection> props))
                throw new Exception($"Your property info was declared by a type '{typeof(T).Name}' that we don't have permissions for.");

            var propertyInfo = typeof(T).GetProperty(propertName) ??
                throw new Exception($"Your property '{propertName}' does not exist on the type '{typeof(T).Name}'.");

            if (!props.TryGetValue(propertyInfo, out PropertyPermissionsCollection permissions))
                return false;

            return CanReturn_Internal(permissions);
        }

        public bool CanReturn(PropertyInfo info)
        {
            if (info.DeclaringType != typeof(T))
                throw new Exception($"The generic type T ({typeof(T).Name}) did not match the declaring type ({info.DeclaringType.Name}) of your property ({info.Name})!");

            if (!PermissionsCache.PermissionTypesCache.TryGetValue(info.DeclaringType, out Dictionary<PropertyInfo, PropertyPermissionsCollection> props))
                throw new Exception($"Your property info was declared by a type '{info.DeclaringType.Name}' that we don't have permissions for.");

            if (!props.TryGetValue(info, out PropertyPermissionsCollection permissions))
                return false;

            return CanReturn_Internal(permissions);
        }

        public bool CanReturn<TValue>(Expression<Func<T, TValue>> selector)
        {
            if (!PermissionsCache.PermissionTypesCache.TryGetValue(typeof(T), out Dictionary<PropertyInfo, PropertyPermissionsCollection> props))
                throw new Exception($"Your type, {typeof(T).Name}, has no permissions declared about it.");

            if (!props.TryGetValue((PropertyInfo)selector.GetProperty(), out PropertyPermissionsCollection permissions))
                return false;

            return CanReturn_Internal(permissions);
        }

        private bool CanReturn_Internal(PropertyPermissionsCollection permissions)
        {
            if (permissions.CanReturnIfSelf && Other != null && Person.Equals(Other))
            {
                return true;
            }

            if (permissions.LevelsRequiredToReturnForChainOfCommand.All(level =>
            {
                if (level.Value != ChainOfCommandLevels.None && Other == null)
                    return false;

                if (_personHighestLevels[level.Key] < level.Value)
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

        public bool CanEdit(string propertName)
        {
            if (!PermissionsCache.PermissionTypesCache.TryGetValue(typeof(T), out Dictionary<PropertyInfo, PropertyPermissionsCollection> props))
                throw new Exception($"Your property info was declared by a type '{typeof(T).Name}' that we don't have permissions for.");

            var propertyInfo = typeof(T).GetProperty(propertName) ??
                throw new Exception($"Your property '{propertName}' does not exist on the type '{typeof(T).Name}'.");

            if (!props.TryGetValue(propertyInfo, out PropertyPermissionsCollection permissions))
                return false;

            return CanEdit_Internal(permissions);
        }

        public bool CanEdit(PropertyInfo info)
        {
            if (info.DeclaringType != typeof(T))
                throw new Exception($"The generic type T ({typeof(T).Name}) did not match the declaring type ({info.DeclaringType.Name}) of your property ({info.Name})!");

            if (!PermissionsCache.PermissionTypesCache.TryGetValue(info.DeclaringType, out Dictionary<PropertyInfo, PropertyPermissionsCollection> props))
                throw new Exception($"Your property info was declared by a type '{info.DeclaringType.Name}' that we don't have permissions for.");

            if (!props.TryGetValue(info, out PropertyPermissionsCollection permissions))
                return false;

            return CanReturn_Internal(permissions);
        }

        public bool CanEdit<TValue>(Expression<Func<T, TValue>> selector)
        {
            if (!PermissionsCache.PermissionTypesCache.TryGetValue(typeof(T), out Dictionary<PropertyInfo, PropertyPermissionsCollection> props))
                throw new Exception($"Your type, {typeof(T).Name}, has no permissions declared about it.");

            if (!props.TryGetValue((PropertyInfo)selector.GetProperty(), out PropertyPermissionsCollection permissions))
                return false;

            return CanEdit_Internal(permissions);
        }

        private bool CanEdit_Internal(PropertyPermissionsCollection permissions)
        {
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

                if (_personHighestLevels[level.Key] < level.Value)
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
