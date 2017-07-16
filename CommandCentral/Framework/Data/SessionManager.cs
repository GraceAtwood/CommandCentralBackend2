using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Caches.SysCache;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Metadata;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace CommandCentral.Framework.Data
{
    public static class SessionManager
    {
        private static ISessionFactory _sessionFactory;

        public static SchemaExport Schema { get; private set; }

        public static ConcurrentDictionary<Type, IClassMetadata> ClassMetaData { get; private set; }

        private static Configuration Config;

        private static object _configLock = new object();

        static SessionManager()
        {
            lock(_configLock)
            {
                if (Config != null)
                    return;

                var mySqlConfig = MySQLConfiguration.Standard.ConnectionString(Utilities.ConfigurationUtility.Configuration.GetConnectionString("Main"));

                if (Utilities.ConfigurationUtility.Configuration.GetValue<bool>("NHibnerate:PrintSQL"))
                {
                    mySqlConfig = mySqlConfig.FormatSql().ShowSql();
                }

                Config = Fluently.Configure()
                    .Database(mySqlConfig)
                    .Cache(x => x.UseSecondLevelCache().UseQueryCache()
                    .ProviderClass<SysCacheProvider>())
                    .Mappings(x => x.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
                    .ExposeConfiguration(x => x.EventListeners.PostLoadEventListeners = new[] { new MyPostLoadListener() })
                    .BuildConfiguration();

                Schema = new SchemaExport(Config);

                var test = Config.BuildSessionFactory();

                ClassMetaData = new ConcurrentDictionary<Type, IClassMetadata>(Config.BuildSessionFactory().GetAllClassMetadata().Select(x => new
                {
                    Type = Assembly.GetExecutingAssembly().GetType(x.Key),
                    MetaData = x.Value
                })
                .ToDictionary(x => x.Type, x => x.MetaData));
            }
        }

        private static ISessionFactory GetFactory<T>() where T : ICurrentSessionContext
        {
            return Config.CurrentSessionContext<T>().BuildSessionFactory();
        }
        
        public static ISession CurrentSession
        {
            get
            {
                if (_sessionFactory == null)
                {
                    if (HttpContext.Current == null)
                    {
                        _sessionFactory = GetFactory<ThreadStaticSessionContext>();
                    }
                    else
                    {
                        _sessionFactory = GetFactory<WebSessionContext>();
                    }
                }

                if (CurrentSessionContext.HasBind(_sessionFactory))
                    return _sessionFactory.GetCurrentSession();
                ISession session = _sessionFactory.OpenSession();
                CurrentSessionContext.Bind(session);
                return session;
            }
        }

        public static void CloseSession()
        {
            if (_sessionFactory == null)
                return;
            if (CurrentSessionContext.HasBind(_sessionFactory))
            {
                ISession session = CurrentSessionContext.Unbind(_sessionFactory);
                session.Close();
            }
        }

        public static void CommitSession(ISession session)
        {
            try
            {
                session.Transaction.Commit();
            }
            catch (Exception)
            {
                session.Transaction.Rollback();
                throw;
            }
        }

    }
}
