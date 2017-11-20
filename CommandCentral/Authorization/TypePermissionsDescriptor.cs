using CommandCentral.Entities;
using CommandCentral.Enums;
using CommandCentral.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Provides methods for determining if a person (Person) can edit or return a given property for another person (Other).
    /// <para />
    /// Important!
    /// <para/>
    /// The best use of this class is to either use the expression overloads of CanReturn and CanEdit in the format "CanReturn(x => x.FirstName)" if interested in permissions about specific properties 
    /// or if interested in all properties for a type, call GetAllPermissions().
    /// </summary>
    /// <typeparam name="T">The type of object in which we will look for permissions.</typeparam>
    public class TypePermissionsDescriptor<T>
    {
        /// <summary>
        /// The principle person for whom permissions are checked.
        /// </summary>
        public Person Person { get; private set; }

        /// <summary>
        /// The person against whom the permissions are checked.
        /// </summary>
        public Person Other { get; private set; }

        /// <summary>
        /// The person's highest levels in each chain of command.
        /// </summary>
        private Dictionary<ChainsOfCommand, ChainOfCommandLevels> _personHighestLevels;

        /// <summary>
        /// The property permissions for all properties in type T.
        /// </summary>
        private Dictionary<PropertyInfo, PropertyPermissionsCollection> _propertyPermissions;

        /// <summary>
        /// Creates a new type permissions descriptor.
        /// </summary>
        /// <param name="person">The principle person for whom to check the permissions.</param>
        /// <param name="other">
        /// The person permissions will be checked against.  
        /// If null, then permissions that require you to be yourself or in a chain of command will return false, 
        /// but permissions that don't require those checks will return true.
        /// </param>
        public TypePermissionsDescriptor(Person person, Person other)
        {
            if (!PermissionsCache.PermissionTypesCache.TryGetValue(typeof(T), out var props))
                throw new ArgumentException($"The given generic type T ({typeof(T).Name}) does not declare permissions.  Add [HasPermissions] above it.", nameof(T));

            _propertyPermissions = props;

            Person = person;
            Other = other;

            _personHighestLevels = person.GetHighestAccessLevels();
        }

        /// <summary>
        /// Returns a set of <seealso cref="PropertyPermissionsDescriptor"/> that describes the permissions for each property.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PropertyPermissionsDescriptor> GetAllPermissions()
        {
            foreach (var pair in _propertyPermissions)
            {
                yield return new PropertyPermissionsDescriptor
                {
                    Property = pair.Key,
                    CanEdit = CanEdit(pair.Value),
                    CanReturn = CanReturn(pair.Value)
                };
            }
        }

        /// <summary>
        /// Returns a boolean indicating if the principle person can return the given property (case sensitive) for the other person.  
        /// Will throw an exception if the requested property does not have permissions declared about it or if the property does not exist.
        /// <para/>
        /// NOTE! This method is less efficient than the other overloads for CanReturn.  Please consider using one of those unless you absolutely have to use a string.
        /// </summary>
        /// <param name="propertyName">The name of the property to check permissions for.</param>
        /// <returns></returns>
        public bool CanReturn(string propertyName)
        {
            var info = _propertyPermissions.Keys.FirstOrDefault(x => x.Name.Equals(propertyName)) ??
                throw new ArgumentException($"Your property, {propertyName}, does not exist within the type permissions for the type '{typeof(T).Name}'", nameof(propertyName));

            return CanReturn_Internal(_propertyPermissions[info]);
        }

        /// <summary>
        /// Returns a boolean indicating if the principle person can return the given property for the other person.  
        /// Will throw an exception if the requested property does not have permissions declared about it or if the property does not exist.
        /// </summary>
        /// <param name="info">The property to check permissions for.</param>
        /// <returns></returns>
        public bool CanReturn(PropertyInfo info)
        {
            if (!_propertyPermissions.TryGetValue(info, out var permissions))
                throw new ArgumentException($"Your property, {info.Name}, does not exist within the type permissions for the type '{typeof(T).Name}'", nameof(info));

            return CanReturn_Internal(permissions);
        }

        /// <summary>
        /// Returns a boolean indicating if the principle person can return the given property for the other person.  
        /// Will throw an exception if the requested property does not have permissions declared about it or if the property does not exist.
        /// </summary>
        /// <param name="selector">The property to check permissions for.  Should be in the form x => x.PropertyName .</param>
        /// <returns></returns>
        public bool CanReturn<TValue>(Expression<Func<T, TValue>> selector)
        {
            var info = (PropertyInfo)selector.GetProperty();
            
            if (!_propertyPermissions.TryGetValue(info, out var permissions))
                throw new ArgumentException($"Your property, {info.Name}, does not exist within the type permissions for the type '{typeof(T).Name}'", nameof(info));

            return CanReturn_Internal(permissions);
        }

        /// <summary>
        /// Returns a boolean indicating if the principle person can return the given property for the other person.  
        /// </summary>
        /// <param name="permissions">The permissions collection that describes permissions for the property in question.</param>
        /// <returns></returns>
        public bool CanReturn(PropertyPermissionsCollection permissions)
        {
            return CanReturn_Internal(permissions);
        }

        /// <summary>
        /// Returns true or false, indicating if the person can return the property for the other person based on all of the properties within the <seealso cref="PropertyPermissionsCollection"/>.
        /// </summary>
        /// <param name="permissions">The permissions collection that describes permissions for the property in question.</param>
        /// <returns></returns>
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
                        if (_personHighestLevels[level.Key] == ChainOfCommandLevels.Command && !Person.IsInSameCommandAs(Other) ||
                            _personHighestLevels[level.Key] == ChainOfCommandLevels.Department && !Person.IsInSameDepartmentAs(Other))
                            return false;
                        break;
                    case ChainOfCommandLevels.Division:
                        if (_personHighestLevels[level.Key] == ChainOfCommandLevels.Command && !Person.IsInSameCommandAs(Other) ||
                            _personHighestLevels[level.Key] == ChainOfCommandLevels.Department && !Person.IsInSameDepartmentAs(Other) ||
                            _personHighestLevels[level.Key] == ChainOfCommandLevels.Division && !Person.IsInSameDivisionAs(Other))
                            return false;
                        break;
                    case ChainOfCommandLevels.None:
                        break;
                    default:
                        throw new NotImplementedException($"Fell to default of switch in {nameof(CanReturn_Internal)}.  Value: {level.Value}");
                }

                return true;
            }))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if the principle person can return the given property, and returns the value of that property for the given object instance if the person can, or returns the default value for the value type if the person can not.
        /// </summary>
        /// <typeparam name="TValue">The type of the property to select.</typeparam>
        /// <param name="obj">The object from which to select the value.</param>
        /// <param name="selector">A selector for the desired property.</param>
        /// <returns></returns>
        public TValue GetSafeReturnValue<TValue>(T obj, Expression<Func<T, TValue>> selector)
        {
            return CanReturn(selector) 
                ? selector.Compile()(obj) 
                : default;
        }

        /// <summary>
        /// Determines if the principle person can return the given property, and returns the value of that property for the given object instance if the person can, or returns the redacted value if the person can not.
        /// </summary>
        /// <typeparam name="TValue">The type of the property to select.</typeparam>
        /// <param name="obj">The object from which to select the value.</param>
        /// <param name="redactedValue">A value to use in case the principle person can not return the property in question.</param>
        /// <param name="selector">A selector for the desired property.</param>
        /// <returns></returns>
        public TValue GetSafeReturnValue<TValue>(T obj, TValue redactedValue, Expression<Func<T, TValue>> selector)
        {
            return CanReturn(selector) 
                ? selector.Compile()(obj) 
                : redactedValue;
        }

        /// <summary>
        /// Returns a boolean indicating if the principle person can edit the given property (case sensitive) for the other person.  
        /// Will throw an exception if the requested property does not have permissions declared about it or if the property does not exist.
        /// <para/>
        /// NOTE! This method is less efficient than the other overloads for CanEdit.  Please consider using one of those unless you absolutely have to use a string.
        /// </summary>
        /// <param name="propertyName">The name of the property to check permissions for.</param>
        /// <returns></returns>
        public bool CanEdit(string propertyName)
        {
            var info = _propertyPermissions.Keys.FirstOrDefault(x => x.Name.Equals(propertyName)) ??
                throw new ArgumentException($"Your property, {propertyName}, does not exist within the type permissions for the type '{typeof(T).Name}'", nameof(propertyName));

            return CanEdit_Internal(_propertyPermissions[info]);
        }

        /// <summary>
        /// Returns a boolean indicating if the principle person can edit the given property for the other person.  
        /// Will throw an exception if the requested property does not have permissions declared about it or if the property does not exist.
        /// </summary>
        /// <param name="info">The property to check permissions for.</param>
        /// <returns></returns>
        public bool CanEdit(PropertyInfo info)
        {
            if (!_propertyPermissions.TryGetValue(info, out var permissions))
                throw new ArgumentException($"Your property, {info.Name}, does not exist within the type permissions for the type '{typeof(T).Name}'", nameof(info));

            return CanEdit_Internal(permissions);
        }

        /// <summary>
        /// Returns a boolean indicating if the principle person can edit the given property for the other person.  
        /// Will throw an exception if the requested property does not have permissions declared about it or if the property does not exist.
        /// </summary>
        /// <param name="selector">The property to check permissions for.  Should be in the form x => x.PropertyName .</param>
        /// <returns></returns>
        public bool CanEdit<TValue>(Expression<Func<T, TValue>> selector)
        {
            var info = (PropertyInfo)selector.GetProperty();

            if (!_propertyPermissions.TryGetValue(info, out var permissions))
                throw new ArgumentException($"Your property, {info.Name}, does not exist within the type permissions for the type '{typeof(T).Name}'", nameof(info));

            return CanEdit_Internal(permissions);
        }

        /// <summary>
        /// Returns a boolean indicating if the principle person can edit the given property for the other person.  
        /// </summary>
        /// <param name="permissions">The permissions collection that describes permissions for the property in question.</param>
        /// <returns></returns>
        public bool CanEdit(PropertyPermissionsCollection permissions)
        {
            return CanEdit_Internal(permissions);
        }

        /// <summary>
        /// Returns true or false, indicating if the person can edit the property for the other person based on all of the properties within the <seealso cref="PropertyPermissionsCollection"/>.
        /// </summary>
        /// <param name="permissions">The permissions collection that describes permissions for the property in question.</param>
        /// <returns></returns>
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
                        if (_personHighestLevels[level.Key] == ChainOfCommandLevels.Command  && !Person.IsInSameCommandAs(Other) ||
                            _personHighestLevels[level.Key] == ChainOfCommandLevels.Department && !Person.IsInSameDepartmentAs(Other))
                            return false;
                        break;
                    case ChainOfCommandLevels.Division:
                        if (_personHighestLevels[level.Key] == ChainOfCommandLevels.Command && !Person.IsInSameCommandAs(Other) ||
                            _personHighestLevels[level.Key] == ChainOfCommandLevels.Department && !Person.IsInSameDepartmentAs(Other) ||
                            _personHighestLevels[level.Key] == ChainOfCommandLevels.Division && !Person.IsInSameDivisionAs(Other))
                            return false;
                        break;
                    case ChainOfCommandLevels.None:
                        break;
                    default:
                        throw new NotImplementedException($"Fell to default of switch in {nameof(CanEdit_Internal)}.  Value: {level.Value}");
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
