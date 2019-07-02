using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web;
using NHibernate;
using NHibernate.Cfg;

namespace ELMAapp.DAL
{
    public class DocRepositoryContext
    {
        private static DocRepositoryContext instance;

        private DocRepositoryContext()
        {
        }

        public static DocRepositoryContext GetInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            instance = new DocRepositoryContext();
            return instance;
        }

        public ConcurrentDictionary<string, DocRepository> DocRepositories { get; } =
            new ConcurrentDictionary<string, DocRepository>();

        public IList<string> Owners { get; set; }

        private ISessionFactory admSessionFactory = new Configuration().Configure(
            HttpContext.Current.Server.MapPath(@"\Models\NHibernate\Configuration\admin.cfg.xml")).BuildSessionFactory();

        public void UpdateOwners()
        {
            using (var admSession = admSessionFactory.OpenSession())
            {
                Owners = admSession.CreateSQLQuery(@"select name from master.sys.server_principals")
                    .AddScalar("name", NHibernateUtil.String).List<string>();
            }
        }

        public void AddDbUser(string userName, string password)
        {
            var sql = string.Format(@"          
            CREATE LOGIN[{0}] WITH PASSWORD = N'{1}', DEFAULT_DATABASE =[master], CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF
            ALTER SERVER ROLE [dbcreator] ADD MEMBER [{0}]           
            CREATE USER[{0}] FOR LOGIN[{0}]
            ALTER USER[{0}] WITH DEFAULT_SCHEMA =[db_owner]
            ALTER ROLE[db_owner] ADD MEMBER[{0}]
            select [name] from master.sys.server_principals"
                , userName, password);
            using (var admSession = admSessionFactory.OpenSession())
            {
                Owners = admSession.CreateSQLQuery(sql).AddScalar("name", NHibernateUtil.String).List<string>();
            }

            if (!Owners.Contains(userName))
            {
                throw new Exception($@"failed to create database user : {userName}");
            }
        }
    }
}