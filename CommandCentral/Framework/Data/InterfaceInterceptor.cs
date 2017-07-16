using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Framework.Data
{
    public class InterfaceInterceptor : EmptyInterceptor
    {
        public override object Instantiate(string clazz, EntityMode entityMode, object id)
        {
            return base.Instantiate(clazz, entityMode, id);
        }
    }
}
