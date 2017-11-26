using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CommandCentral.Entities;

namespace CommandCentral.Authorization
{
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

        public static bool CanEdit<T>(this Person editor, T entity, Expression<Func<T, object>> propertySelector)
            where T : Entity
        {
            if (!ContractsByType.TryGetValue(typeof(T), out var contract))
                throw new Exception("No contract found.");

            return ((RulesContract<T>) contract).CanEditProperty(editor, entity, propertySelector);
        }
        
        public static bool CanReturn<T>(this Person editor, T entity, Expression<Func<T, object>> propertySelector)
            where T : Entity
        {
            if (!ContractsByType.TryGetValue(typeof(T), out var contract))
                throw new Exception("No contract found.");

            return ((RulesContract<T>) contract).CanReturnProperty(editor, entity, propertySelector);
        }
    }
}