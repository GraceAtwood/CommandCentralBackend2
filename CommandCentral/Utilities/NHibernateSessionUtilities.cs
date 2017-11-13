using NHibernate;
using NHibernate.Engine;
using NHibernate.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Entities;
using NHibernate.Type;
using System.Linq.Expressions;

namespace CommandCentral.Utilities
{
    /// <summary>
    /// A collection of extensions to NHibernate's session object.
    /// </summary>
    public static class NHibernateUtilities
    {
        /// <summary>
        /// Gets the real, underlying Entity-type - as opposed to the standard GetType() method,
        /// this method takes into account the possibility that the object may in fact be an
        /// NHibernate Proxy object, and not a real object. This method will return the real
        /// Entity-type, doing a full initialization if necessary.
        /// </summary>
        public static Type GetEntityType(this Entity entity, IPersistenceContext persistenceContext)
        {
            if (!(entity is INHibernateProxy)) 
                return entity.GetType();
            
            var lazyInitialiser = ((INHibernateProxy)entity).HibernateLazyInitializer;
            var type = lazyInitialiser.PersistentClass;

            if (type.IsAbstract || type.GetNestedTypes().Length > 0)
                return (entity is INHibernateProxy
                    ? persistenceContext.Unproxy(entity)
                    : entity).GetType();
            
            return lazyInitialiser.PersistentClass;
        }
        
        /// <summary>
        /// Returns a collection of properties that are dirty along with their new and old values.
        /// <para />
        /// This has only been tested on the Person object, but technically it should work on anything.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static IEnumerable<Change> GetChangesFromDirtyProperties<T>(this ISession session, T entity) where T : class, new()
        {
            var entityName = session.GetSessionImplementation().Factory.TryGetGuessEntityName(typeof(T)) ??
                throw new Exception($"We attempted to find the entity name for a non-entity: {typeof(T)}");

            var persister = session.GetSessionImplementation().GetEntityPersister(entityName, entity);
            var key = new EntityKey(persister.GetIdentifier(entity), persister);
            var entityEntry = session.GetSessionImplementation().PersistenceContext.GetEntry(session.GetSessionImplementation().PersistenceContext.GetEntity(key));

            var currentState = persister.GetPropertyValues(entity);

            //Find dirty will give us all the properties that are dirty, but because of some grade A NHibernate level bullshit, it won't look at collection for us.
            var indices = persister.FindDirty(currentState.ToArray(), entityEntry.LoadedState, entity, session.GetSessionImplementation());

            if (indices != null)
            {
                foreach (var index in indices)
                {
                    yield return new Change
                    {
                        NewValue = currentState[index]?.ToString(),
                        OldValue = entityEntry.LoadedState[index]?.ToString(),
                        PropertyName = persister.PropertyNames[index],
                        Id = Guid.NewGuid()
                    };
                }
            }

            //Here we walk through the collections ourselves in order to determine what has changed.
            for (var x = 0; x < persister.PropertyTypes.Length; x++)
            {
                if (!(persister.PropertyTypes[x] is CollectionType)) 
                    continue;
                
                if (!CollectionUtilities.ScrambledEquals((dynamic)currentState[x], (dynamic)entityEntry.LoadedState[x]))
                {
                    yield return new Change
                    {
                        Id = Guid.NewGuid(),
                        NewValue = String.Join(", ", (dynamic)currentState[x]),
                        OldValue = String.Join(", ", (dynamic)entityEntry.LoadedState[x]),
                        PropertyName = persister.PropertyNames[x]
                    };
                }
            }
        }

        /// <summary>
        /// Returns the loaded value for a given property name of a given entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="session"></param>
        /// <param name="entity"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static TProperty GetLoadedPropertyValue<T, TProperty>(this ISession session, T entity, Expression<Func<T, TProperty>> selector) where T: class
        {

            var entityName = session.GetSessionImplementation().Factory.TryGetGuessEntityName(typeof(T)) ??
                throw new Exception($"We attempted to find the entity name for a non-entity: {typeof(T)}");

            var persister = session.GetSessionImplementation().GetEntityPersister(entityName, entity);
            var key = new EntityKey(persister.GetIdentifier(entity), persister);
            var entityEntry = session.GetSessionImplementation().PersistenceContext.GetEntry(session.GetSessionImplementation().PersistenceContext.GetEntity(key));

            return (TProperty)entityEntry.GetLoadedValue((selector.Body as MemberExpression)?.Member.Name);
        }
    }
}
