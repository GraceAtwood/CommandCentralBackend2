using CommandCentral.Authorization;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System;
using System.Data;
using System.Data.Common;
using NHibernate.Engine;

namespace CommandCentral.Utilities.Types
{
    [Serializable]
    public class CustomPermissionGroupType : IUserType
    {

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x == null || y == null)
                return false;

            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return x == null 
                ? 0 
                : x.GetHashCode();
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            if (names.Length == 0)
                throw new ArgumentException("Expecting at least one column!", nameof(names));

            var name = NHibernateUtil.String.NullSafeGet(rs, names[0], session) as string;

            if (!PermissionsCache.PermissionGroupsCache.TryGetValue(name, out var group))
                throw new Exception($"Unable to find permission group named {name}");

            return group;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            var parameter = (DbParameter)cmd.Parameters[index];

            if (value == null)
                parameter.Value = 0;
            else
                parameter.Value = ((PermissionGroup)value).Name;
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public SqlType[] SqlTypes => new[] { new SqlType(DbType.Guid) };

        public Type ReturnedType => typeof(PermissionGroup);

        public bool IsMutable => false;
    }
}
