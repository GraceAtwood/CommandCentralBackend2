using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using NHibernate;
using NHibernate.Caches.SysCache;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Metadata;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using CommandCentral.Utilities;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;
using ISession = NHibernate.ISession;

namespace CommandCentral.Framework.Data
{
    /// <summary>
    /// Provides management and access to the current session for this context.  Context is derived from either the current HTTP context or the current thead static context.
    /// </summary>
    public static class SessionManager
    {
        private static ISessionFactory _sessionFactory;

        /// <summary>
        /// This object contains a description of the database schema as NHibnerate expects it to look.  
        /// In an ideal situation, if the database hasn't been updated manually, this schema will be an in-code representation of the schema as it exists in the database.
        /// </summary>
        public static SchemaExport Schema { get; }

        private static readonly Configuration _config;

        private static readonly object _configLock = new object();

        static SessionManager()
        {
            lock(_configLock)
            {
                if (_config != null)
                    return;

                var mySqlConfig = MySQLConfiguration.Standard.ConnectionString(ConfigurationUtility.Configuration.GetConnectionString("Main"));
                
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
            }
        }

        private static ISessionFactory GetFactory<T>() where T : ICurrentSessionContext
        {
            return _config.CurrentSessionContext<T>().BuildSessionFactory();
        }
        
        /// <summary>
        /// Returns the current session associated with the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ISession GetCurrentSession(HttpContext context = null)
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
            session.FlushMode = FlushMode.Commit;
            CurrentSessionContext.Bind(session);
            return session;
        }

        /// <summary>
        /// Unbinds the session from the current session context.  After this call, calls to GetCurrentSession will fail for the current session.
        /// </summary>
        public static void UnbindSession()
        {
            if (_sessionFactory == null)
                return;

            if (!CurrentSessionContext.HasBind(_sessionFactory)) 
                return;
            
            CurrentSessionContext.Unbind(_sessionFactory);
        }
    }
}
