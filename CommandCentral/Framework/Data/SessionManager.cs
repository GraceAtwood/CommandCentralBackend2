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
using CommandCentral.Utilities;
using Microsoft.AspNetCore.Http;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;
using ISession = NHibernate.ISession;

namespace CommandCentral.Framework.Data
{
    public static class SessionManager
    {
        private static ISessionFactory _sessionFactory;

        public static SchemaExport Schema { get; }

        public static ConcurrentDictionary<Type, IClassMetadata> ClassMetaData { get; }

        private static readonly Configuration _config;

        private static readonly object _configLock = new object();

        static SessionManager()
        {
            lock(_configLock)
            {
                if (_config != null)
                    return;

                var mySqlConfig = MySQLConfiguration.Standard.ConnectionString(Utilities.ConfigurationUtility.Configuration.GetConnectionString("Main"));
                
                // If appsettings.json says we're in debug, show SQL in the CLI
                if (Boolean.TryParse(ConfigurationUtility.Configuration["DebugMode"], out bool debugMode) && debugMode)
                    mySqlConfig = mySqlConfig.FormatSql().ShowSql();

                _config = Fluently.Configure()
                    .Database(mySqlConfig)
                    .Cache(x => x.UseSecondLevelCache().UseQueryCache()
                    .ProviderClass<SysCacheProvider>())
                    .Mappings(x => x.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
                    .ExposeConfiguration(x => x.EventListeners.PostLoadEventListeners = new[] { new MyPostLoadListener() })
                    .BuildConfiguration();

                Schema = new SchemaExport(_config);

                ClassMetaData = new ConcurrentDictionary<Type, IClassMetadata>(_config.BuildSessionFactory().GetAllClassMetadata().Select(x => new
                {
                    Type = Assembly.GetExecutingAssembly().GetType(x.Key),
                    MetaData = x.Value
                })
                .ToDictionary(x => x.Type, x => x.MetaData));
            }
        }

        private static ISessionFactory GetFactory<T>() where T : ICurrentSessionContext
        {
            return _config.CurrentSessionContext<T>().BuildSessionFactory();
        }
        
        public static ISession CurrentSession(HttpContext context = null)
        {
            if (_sessionFactory == null)
            {
                _sessionFactory = context == null 
                    ? GetFactory<ThreadStaticSessionContext>() 
                    : GetFactory<WebSessionContext>();
            }

            if (CurrentSessionContext.HasBind(_sessionFactory))
                return _sessionFactory.GetCurrentSession();
            
            var session = _sessionFactory.OpenSession();
            CurrentSessionContext.Bind(session);
            return session;
        }

        public static void CloseSession()
        {
            if (_sessionFactory == null)
                return;

            if (!CurrentSessionContext.HasBind(_sessionFactory)) 
                return;
            
            var session = CurrentSessionContext.Unbind(_sessionFactory);
            session.Close();
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
