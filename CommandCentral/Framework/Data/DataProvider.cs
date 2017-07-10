using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
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
    public static class DataProvider
    {
        public static ISessionFactory SessionFactory { get; private set; }

        public static SchemaExport Schema { get; private set; }

        public static Configuration Config { get; private set; }

        public static ConcurrentDictionary<Type, IClassMetadata> ClassMetaData { get; private set; }

        public static string ConnectionString { get; set; }

        private static ISessionFactory GetFactory<T>() where T : ICurrentSessionContext
        {
            Config = Fluently.Configure()
                .Database(MySQLConfiguration.Standard.ConnectionString(ConnectionString)
                //.ShowSql()
                )
                .Cache(x => x.UseSecondLevelCache().UseQueryCache()
                .ProviderClass<SysCacheProvider>())
                .Mappings(x => x.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
                .CurrentSessionContext<T>()
                .BuildConfiguration();


            //We're going to save the schema in case the host wants to use it later.
            Schema = new SchemaExport(Config);

            var factory = Config.BuildSessionFactory();

            ClassMetaData = new ConcurrentDictionary<Type, IClassMetadata>(factory.GetAllClassMetadata().Select(x => new
            {
                Type = Assembly.GetExecutingAssembly().GetType(x.Key),
                MetaData = x.Value
            })
            .ToDictionary(x => x.Type, x => x.MetaData));

            return factory;
        }

        public static void ForceFactoryInitialization()
        {
            if (SessionFactory == null)
                SessionFactory = HttpContext.Current != null
                                ? GetFactory<WebSessionContext>()
                                : GetFactory<ThreadStaticSessionContext>();
        }
        
        public static ISession CurrentSession
        {
            get
            {
                if (SessionFactory == null)
                    SessionFactory = HttpContext.Current != null
                                    ? GetFactory<WebSessionContext>()
                                    : GetFactory<ThreadStaticSessionContext>();
                if (CurrentSessionContext.HasBind(SessionFactory))
                    return SessionFactory.GetCurrentSession();
                ISession session = SessionFactory.OpenSession();
                CurrentSessionContext.Bind(session);
                return session;
            }
        }

        public static void CloseSession()
        {
            if (SessionFactory == null)
                return;
            if (CurrentSessionContext.HasBind(SessionFactory))
            {
                ISession session = CurrentSessionContext.Unbind(SessionFactory);
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
