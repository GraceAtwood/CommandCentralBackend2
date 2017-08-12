using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Criterion;
using CommandCentral.Framework.Data;
using CommandCentral.Utilities;
using System.Reflection;
using System.Collections.Concurrent;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Provides generalized methods for helping with reference lists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ReferenceListHelper
    {
        /// <summary>
        /// Contains a mapping of all reference list names to their matching types.
        /// </summary>
        public static ConcurrentDictionary<string, Type> ReferenceListNamesToType;
        static ReferenceListHelper()
        {
            ReferenceListNamesToType = new ConcurrentDictionary<string, Type>(
                Assembly.GetExecutingAssembly().GetTypes()
                    .Where(x => typeof(ReferenceListItemBase).IsAssignableFrom(x))
                    .ToDictionary(x => x.Name, x => x, StringComparer.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Returns true or false if the list exists.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Exists<T>(string value) where T : ReferenceListItemBase
        {
            return SessionManager.CurrentSession.QueryOver<T>().Where(x => x.Value.IsInsensitiveLike(value)).RowCount() != 0;
        }

        /// <summary>
        /// Returns true or false if a list with this id exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IdExists<T>(Guid id) where T : ReferenceListItemBase
        {
            return SessionManager.CurrentSession.QueryOver<T>().Where(x => x.Id == id).RowCount() != 0;
        }

        /// <summary>
        /// Returns true or false indicating if all values represent a reference list.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool AllExist<T>(params string[] values) where T : ReferenceListItemBase
        {
            var array = values.Distinct().ToArray();
            return SessionManager.CurrentSession.QueryOver<T>().Where(x => x.Value.IsIn(array)).RowCount() == values.Length;
        }

        /// <summary>
        /// Returns true or false indicating if all values represent a reference list.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool AllExist<T>(IEnumerable<string> values) where T : ReferenceListItemBase
        {
            var array = values.Distinct().ToArray();
            return SessionManager.CurrentSession.QueryOver<T>().Where(x => x.Value.IsIn(array)).RowCount() == array.Length;
        }

        /// <summary>
        /// Returns all reference lists of the given type.
        /// </summary>
        /// <returns></returns>
        public static List<T> All<T>() where T : ReferenceListItemBase
        {
            return (List<T>)SessionManager.CurrentSession.QueryOver<T>().List();
        }

        /// <summary>
        /// Returns a reference list whose value is the requested value or throws an exception if none are found.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Find<T>(string value) where T : ReferenceListItemBase
        {
            return SessionManager.CurrentSession.QueryOver<T>().Where(x => x.Value.IsInsensitiveLike(value))
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
            foreach (var value in values)
            {
                yield return Find<T>(value);
            }
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

            return SessionManager.CurrentSession.Get<T>(result);
        }

        /// <summary>
        /// Returns a random selection of elements from this reference list.
        /// </summary>
        /// <param name="count">The number of elements to return.  If the count is greater than the total number of lists, the total number will be returned instead.</param>
        /// <returns></returns>
        public static IEnumerable<T> Random<T>(int count) where T : ReferenceListItemBase
        {
            var list = SessionManager.CurrentSession.QueryOver<T>().List();

            if (count > list.Count)
                count = list.Count;

            return list.Shuffle().Take(count);
        }
    }
}
