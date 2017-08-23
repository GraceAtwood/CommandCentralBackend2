using CommandCentral.Authorization;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

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

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            if (names.Length == 0)
                throw new ArgumentException("Expecting at least one column!", nameof(names));

            var name = NHibernateUtil.String.NullSafeGet(rs, names[0]) as string;

            if (!PermissionsCache.PermissionGroupsCache.TryGetValue(name, out PermissionGroup group))
                throw new Exception($"Unable to find permission group named {name}");

            return group;
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
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
