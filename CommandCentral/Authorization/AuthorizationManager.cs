using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CommandCentral.Entities;
using CommandCentral.Entities.CollateralDutyTracking;
using CommandCentral.Enums;
using CommandCentral.Framework;
using CommandCentral.Framework.Data;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Holds the rules contracts cache and provides methods for determing a person's access to various entities.
    /// </summary>
    public static class AuthorizationManager
    {
        private static ConcurrentDictionary<Type, BaseRulesContract> ContractsByType { get; }

        static AuthorizationManager()
        {
            ContractsByType = new ConcurrentDictionary<Type, BaseRulesContract>(
                Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => (typeof(BaseRulesContract).IsAssignableFrom(type) || type.GetInterfaces().Any(x =>
                                        x.IsGenericType && x.GetGenericTypeDefinition() == typeof(BaseRulesContract))
                                   ) && type != typeof(RulesContract<>) && type != typeof(BaseRulesContract))
                    .ToDictionary(type => type.BaseType.GenericTypeArguments.First(),
                        type =>
                            Activator.CreateInstance(type) as BaseRulesContract));
        }

        private static RulesContract<T> GetContract<T>(Type entityType) where T : Entity
        {
            if (!ContractsByType.TryGetValue(entityType, out var contract))
                throw new Exception($"No contract for type: {entityType.Name}");

            return (RulesContract<T>) contract;
        }

        /// <summary>
        /// Determines if this person can edit the given property of the given entity.
        /// </summary>
        /// <param name="editor">The person for whom to check permissions.</param>
        /// <param name="entity">The entity against which to check permissions.</param>
        /// <param name="propertySelector">A selector for the property this person wishes to edit.</param>
        /// <typeparam name="T">Any type that derives from <seealso cref="Entity"/></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">If a rules contract for the type T is not found.</exception>
        public static bool CanEdit<T>(this Person editor, T entity, Expression<Func<T, object>> propertySelector)
            where T : Entity
        {
            return GetContract<T>(entity.GetTypeUnproxied()).CanEditProperty(editor, entity, propertySelector);
        }
        
        /// <summary>
        /// Determines if this person can edit the given property of the given entity.
        /// </summary>
        /// <param name="editor">The person for whom to check permissions.</param>
        /// <param name="entity">The entity against which to check permissions.</param>
        /// <param name="propertyName">The property this person wishes to edit.</param>
        /// <typeparam name="T">Any type that derives from <seealso cref="Entity"/></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">If a rules contract for the type T is not found.</exception>
        public static bool CanEdit<T>(this Person editor, T entity, string propertyName)
            where T : Entity
        {
            return GetContract<T>(entity.GetTypeUnproxied()).CanEditProperty(editor, entity, propertyName);
        }

        /// <summary>
        /// Determines if this person can edit ANY property of  the given entity.
        /// </summary>
        /// <param name="person">The person for whom to check permissions.</param>
        /// <param name="entity">The entity against whicch to check permissions.</param>
        /// <typeparam name="T">Any type that derives from <seealso cref="Entity"/></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">If a rules contract for the type T is not found.</exception>
        public static bool CanEdit<T>(this Person person, T entity) where T : Entity
        {
            return GetContract<T>(entity.GetTypeUnproxied()).CanEditRuleOverride(person, entity);
        }

        /// <summary>
        /// Determines if this person can return ANY property of  the given entity.
        /// </summary>
        /// <param name="person">The person for whom to check permissions.</param>
        /// <param name="entity">The entity against whicch to check permissions.</param>
        /// <typeparam name="T">Any type that derives from <seealso cref="Entity"/></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">If a rules contract for the type T is not found.</exception>
        public static bool CanReturn<T>(this Person person, T entity) where T : Entity
        {
            return GetContract<T>(entity.GetTypeUnproxied()).CanReturnRuleOverride(person, entity);
        }

        /// <summary>
        /// Determines if this person can return the given property of the given entity.
        /// </summary>
        /// <param name="editor">The person for whom to check permissions.</param>
        /// <param name="entity">The entity against which to check permissions.</param>
        /// <param name="propertySelector">A selector for the property this person wishes to return.</param>
        /// <typeparam name="T">Any type that derives from <seealso cref="Entity"/></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">If a rules contract for the type T is not found.</exception>
        public static bool CanReturn<T>(this Person editor, T entity, Expression<Func<T, object>> propertySelector)
            where T : Entity
        {
            return GetContract<T>(entity.GetTypeUnproxied()).CanReturnProperty(editor, entity, propertySelector);
        }

        /// <summary>
        /// Determines if this person can return the given property of the given entity.
        /// </summary>
        /// <param name="editor">The person for whom to check permissions.</param>
        /// <param name="entity">The entity against which to check permissions.</param>
        /// <param name="propertyName">The name of the property this person wishes to return.</param>
        /// <typeparam name="T">Any type that derives from <seealso cref="Entity"/></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">If a rules contract for the type T is not found.</exception>
        public static bool CanReturn<T>(this Person editor, T entity, string propertyName)
            where T : Entity
        {
            return GetContract<T>(entity.GetTypeUnproxied()).CanReturnProperty(editor, entity, propertyName);
        }

        /// <summary>
        /// Determines if this person can delete the given objecct.
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool CanDelete<T>(this Person editor, T entity) where T : Entity
        {
            return GetContract<T>(entity.GetTypeUnproxied()).CanDeleteRule(editor, entity);
        }

        /// <summary>
        /// Determines if this person can create the given object (persist it in the database).
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool CanCreate<T>(this Person editor, T entity) where T : Entity
        {
            return GetContract<T>(entity.GetTypeUnproxied()).CanCreateRule(editor, entity);
        }

        /// <summary>
        /// Returns the highest levels at which this person can access the given chains of command.
        /// </summary>
        /// <param name="person">The person for whom to check permissions.</param>
        /// <returns></returns>
        public static Dictionary<ChainsOfCommand, ChainOfCommandLevels> GetHighestAccessLevels(this Person person)
        {
            var result = ((ChainsOfCommand[]) Enum.GetValues(typeof(ChainsOfCommand)))
                .ToDictionary(x => x, x => ChainOfCommandLevels.None);

            var memberships = SessionManager.GetCurrentSession().Query<CollateralDutyMembership>()
                .Where(x => x.Person == person);

            foreach (var membership in memberships)
            {
                if (result[membership.CollateralDuty.ChainOfCommand] < membership.Level)
                    result[membership.CollateralDuty.ChainOfCommand] = membership.Level;
            }

            return result;
        }

        /// <summary>
        /// Determines if this person can access the given chain of command at or above the given level.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="chainOfCommand"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static bool IsInChainOfCommandAtLevel(this Person person, ChainsOfCommand chainOfCommand,
            ChainOfCommandLevels level)
        {
            return person.GetHighestAccessLevels()[chainOfCommand] >= level;
        }

        /// <summary>
        /// Determines if this person is in the chain of command of the other person.  
        /// </summary>
        /// <param name="person">The person for whom to check permissions.</param>
        /// <param name="other">The person against whom to check the persons.</param>
        /// <param name="chainsOfCommand">A list of the chains of command to check.  
        /// If left blank, ANY chain of command will satisfy the condition.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">If a chain of command is not handled.</exception>
        public static bool IsInChainOfCommand(this Person person, Person other,
            params ChainsOfCommand[] chainsOfCommand)
        {
            if (chainsOfCommand == null || !chainsOfCommand.Any())
                chainsOfCommand = (ChainsOfCommand[]) Enum.GetValues(typeof(ChainsOfCommand));
            foreach (var pair in person.GetHighestAccessLevels().Where(x => chainsOfCommand.Contains(x.Key)))
            {
                switch (pair.Value)
                {
                    case ChainOfCommandLevels.Command:
                    {
                        if (person.Division.Department.Command == other.Division.Department.Command)
                            return true;

                        break;
                    }
                    case ChainOfCommandLevels.Department:
                    {
                        if (person.Division.Department == other.Division.Department)
                            return true;

                        break;
                    }
                    case ChainOfCommandLevels.Division:
                    {
                        if (person.Division == other.Division)
                            return true;

                        break;
                    }
                    case ChainOfCommandLevels.None:
                    {
                        //None does not result in a check.
                        break;
                    }
                    default:
                        throw new NotImplementedException("Fell to default in is in chain of command check.");
                }
            }

            return false;
        }
    }
}