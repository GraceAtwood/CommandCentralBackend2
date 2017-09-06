using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using System.Reflection;
using System.Collections.Concurrent;
using NHibernate.Linq;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Provides generalized methods for helping with reference lists.
    /// </summary>
    public static class ReferenceListHelper
    {
        /// <summary>
        /// Contains a mapping of all reference list names to their matching types.
        /// </summary>
        public static readonly ConcurrentDictionary<string, Type> ReferenceListNamesToType;
        static ReferenceListHelper()
        {
            ReferenceListNamesToType = new ConcurrentDictionary<string, Type>(
                Assembly.GetExecutingAssembly().GetTypes()
                    .Where(x => typeof(ReferenceListItemBase).IsAssignableFrom(x))
                    .ToDictionary(x => x.Name, x => x), StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Returns true or false if the list exists.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Exists<T>(string value) where T : ReferenceListItemBase
        {
            return SessionManager.GetCurrentSession().Query<T>().Count(x => x.Value == value) != 0;
        }

        /// <summary>
        /// Returns true or false if a list with this id exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IdExists<T>(Guid id) where T : ReferenceListItemBase
        {
            return SessionManager.GetCurrentSession().Query<T>().Count(x => x.Id == id) != 0;
        }

        /// <summary>
        /// Returns true or false indicating if all values represent a reference list.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool AllExist<T>(params string[] values) where T : ReferenceListItemBase
        {
            return SessionManager.GetCurrentSession().Query<T>().Count(x => values.Contains(x.Value)) == values.Length;
        }

        /// <summary>
        /// Returns true or false indicating if all values represent a reference list.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool AllExist<T>(IEnumerable<string> values) where T : ReferenceListItemBase
        {
            var list = values.ToList();
            return SessionManager.GetCurrentSession().Query<T>().Count(x => list.Contains(x.Value)) == list.Count();
        }

        /// <summary>
        /// Returns all reference lists of the given type.
        /// </summary>
        /// <returns></returns>
        public static List<T> All<T>() where T : ReferenceListItemBase
        {
            return (List<T>)SessionManager.GetCurrentSession().Query<T>().ToList();
        }

        /// <summary>
        /// Returns a reference list whose value is the requested value or throws an exception if none are found.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Find<T>(string value) where T : ReferenceListItemBase
        {
            return SessionManager.GetCurrentSession().Query<T>().Where(x => x.Value == value)
                .Cacheable()
                .SingleOrDefault() ??
                throw new Exception($"Failed to find reference list {value} of type {typeof(T).Name}");
        }

        /// <summary>
        /// Returns all reference lists whose values match those passed and throws an exception if one or more are not found.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindAll<T>(params string[] values) where T : ReferenceListItemBase
        {
            return values.Select(Find<T>);
        }

        /// <summary>
        /// Gets a reference list with the given id or returns null.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Get<T>(string id) where T : ReferenceListItemBase
        {
            if (!Guid.TryParse(id, out Guid result))
                return null;

            return SessionManager.GetCurrentSession().Get<T>(result);
        }

        /// <summary>
        /// Returns a random selection of elements from this reference list.
        /// </summary>
        /// <param name="count">The number of elements to return.  If the count is greater than the total number of lists, the total number will be returned instead.</param>
        /// <returns></returns>
        public static IEnumerable<T> Random<T>(int count) where T : ReferenceListItemBase
        {
            var list = SessionManager.GetCurrentSession().Query<T>().ToList();

            if (count > list.Count)
                count = list.Count;

            return list.Shuffle().Take(count);
        }
    }
}
