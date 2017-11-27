using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CommandCentral.Entities;
using CommandCentral.Entities.CollateralDutyTracking;
using CommandCentral.Enums;
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
                    .Where(type => typeof(BaseRulesContract).IsAssignableFrom(type) &&
                                   type != typeof(BaseRulesContract))
                    .ToDictionary(type => type, type => (BaseRulesContract) Activator.CreateInstance(type)));
        }

        /// <summary>
        /// Determines if this person can edit the given property of the given entity.
        /// </summary>
        /// <param name="editor">The person for whom to check permissions.</param>
        /// <param name="entity">The entity against which to check permissions.</param>
        /// <param name="propertySelector">A selector for the property this person wishes to edit.</param>
        /// <typeparam name="T">Any type that derives from <seealso cref="Entities.Entity"/></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">If a rules contract for the type T is not found.</exception>
        public static bool CanEdit<T>(this Person editor, T entity, Expression<Func<T, object>> propertySelector)
            where T : Entity
        {
            if (!ContractsByType.TryGetValue(typeof(T), out var contract))
                throw new Exception("No contract found.");

            return ((RulesContract<T>) contract).CanEditProperty(editor, entity, propertySelector);
        }

        /// <summary>
        /// Determines if this person can edit ANY property of  the given entity.
        /// </summary>
        /// <param name="person">The person for whom to check permissions.</param>
        /// <param name="entity">The entity against whicch to check permissions.</param>
        /// <typeparam name="T">Any type that derives from <seealso cref="Entities.Entity"/></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">If a rules contract for the type T is not found.</exception>
        public static bool CanEdit<T>(this Person person, T entity) where T : Entity
        {
            if (!ContractsByType.TryGetValue(typeof(T), out var contract))
                throw new Exception("No contract found.");

            return ((RulesContract<T>) contract).CanEditAnyProperty(person, entity);
        }
        
        /// <summary>
        /// Determines if this person can return ANY property of  the given entity.
        /// </summary>
        /// <param name="person">The person for whom to check permissions.</param>
        /// <param name="entity">The entity against whicch to check permissions.</param>
        /// <typeparam name="T">Any type that derives from <seealso cref="Entities.Entity"/></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">If a rules contract for the type T is not found.</exception>
        public static bool CanReturn<T>(this Person person, T entity) where T : Entity
        {
            if (!ContractsByType.TryGetValue(typeof(T), out var contract))
                throw new Exception("No contract found.");

            return ((RulesContract<T>) contract).CanReturnAnyProperty(person, entity);
        }

        /// <summary>
        /// Determines if this person can return the given property of the given entity.
        /// </summary>
        /// <param name="editor">The person for whom to check permissions.</param>
        /// <param name="entity">The entity against which to check permissions.</param>
        /// <param name="propertySelector">A selector for the property this person wishes to return.</param>
        /// <typeparam name="T">Any type that derives from <seealso cref="Entities.Entity"/></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">If a rules contract for the type T is not found.</exception>
        public static bool CanReturn<T>(this Person editor, T entity, Expression<Func<T, object>> propertySelector)
            where T : Entity
        {
            if (!ContractsByType.TryGetValue(typeof(T), out var contract))
                throw new Exception("No contract found.");

            return ((RulesContract<T>) contract).CanReturnProperty(editor, entity, propertySelector);
        }
        
        /// <summary>
        /// Determines if this person can return the given property of the given entity.
        /// </summary>
        /// <param name="editor">The person for whom to check permissions.</param>
        /// <param name="entity">The entity against which to check permissions.</param>
        /// <param name="propertyName">The name of the property this person wishes to return.</param>
        /// <typeparam name="T">Any type that derives from <seealso cref="Entities.Entity"/></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">If a rules contract for the type T is not found.</exception>
        public static bool CanReturn<T>(this Person editor, T entity, string propertyName)
            where T : Entity
        {
            if (!ContractsByType.TryGetValue(typeof(T), out var contract))
                throw new Exception("No contract found.");

            return ((RulesContract<T>) contract).CanReturnProperty(editor, entity, propertyName);
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
                        if (person.Command == other.Command)
                            return true;

                        break;
                    }
                    case ChainOfCommandLevels.Department:
                    {
                        if (person.Department == other.Department)
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