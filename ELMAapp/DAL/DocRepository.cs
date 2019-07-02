using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using ELMAapp.Models;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Linq;
using WebMatrix.WebData;

namespace ELMAapp.DAL
{
    public class DocRepository
    {
        private static IList<string> owners;

        private static ISessionFactory admSessionFactory = new Configuration().Configure(
            HttpContext.Current.Server.MapPath(@"\Models\NHibernate\Configuration\admin.cfg.xml")).BuildSessionFactory();

        private static void UpdateOwners()
        {
            using (var admSession = admSessionFactory.OpenSession())
            {
                owners = admSession.CreateSQLQuery(@"select name from master.sys.server_principals")
                    .AddScalar("name", NHibernateUtil.String).List<string>();
            }
        }

        private static void AddDbUser(string userName, string password)
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
                owners = admSession.CreateSQLQuery(sql).AddScalar("name", NHibernateUtil.String).List<string>();
            }

            if (!owners.Contains(userName))
            {
                throw new Exception($@"failed to create database user : {userName}");
            }
        }

        private class LoginSessionFactory
        {
            public LoginSessionFactory(ISessionFactory sessionFactory, string userName)
            {
                SessionFactory = sessionFactory;
                UserName = userName;
            }

            private ISessionFactory SessionFactory { get; }
            private string UserName { get; }

            public ISession OpenSession()
            {
                return SessionFactory.OpenSession();
            }

            public bool LoginCheck()
            {
                var currentUser = Membership.GetUser();
                return (currentUser != null && !string.IsNullOrEmpty(currentUser.UserName) && UserName == currentUser.UserName);
            }
        }

        private LoginSessionFactory loginSessionFactory;

        private LoginSessionFactory CreateSessionFactory()
        {
            var currentUser = Membership.GetUser();
            if (string.IsNullOrEmpty(currentUser?.UserName))
            {
                throw new Exception("Authorization Error");
            }

            string password;
            using (var usersContext = new UsersContext())
            {
                password = usersContext.UserProfiles.Where(up => up.UserId == WebSecurity.CurrentUserId).First()
                    .SqlPassword;
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new Exception("Authorization Error");
            }

            if (owners == null)
            {
                UpdateOwners();
            }

            if (owners == null)
            {
                throw new Exception("MSSQL did not provide more than one login");
            }

            if (!owners.Contains(currentUser.UserName))
            {
                AddDbUser(currentUser.UserName, password);
            }

            var cgf = new Configuration();
            var data = cgf.Configure(HttpContext.Current.Server.MapPath(@"\Models\NHibernate\Configuration\user.cfg.xml"));
            cgf.AddDirectory(new System.IO.DirectoryInfo(HttpContext.Current.Server.MapPath(@"\Models\NHibernate\Mappings")));
            cgf.SetProperty(
                "connection.connection_string",
                @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AppELMA.mdf;" +
                $"User Id ={currentUser.UserName};Password={password};"
            );
            return new LoginSessionFactory(data.BuildSessionFactory(), currentUser.UserName);
        }

        private ISession OpenSession()
        {
            if (loginSessionFactory == null || loginSessionFactory.LoginCheck())
            {
                loginSessionFactory = CreateSessionFactory();
            }

            return loginSessionFactory.OpenSession();
        }

        public static readonly List<string> SortedFields = new List<string>() {"Name", "Author", "Date"};
        public static readonly List<string> SelectFields = new List<string>() {"Name", "Author", "BinaryFile", "All Fields"};

        public IEnumerable<Documents> SearchAndSortDocuments(
            bool reverse, string sortBy, string selectField, string searchString, DateTime startDate, DateTime endDate
        )
        {
            if (searchString == null) searchString = "";
            if (!SortedFields.Contains(sortBy)) sortBy = "Name";
            using (var session = OpenSession())
            {
                return session.Query<Documents>().Where(d =>
                        (d.Date >= startDate && d.Date < endDate) &&
                        ((!SelectFields.Contains(selectField) || searchString == "") ||
                         ((selectField == "Name" || selectField == "All Fields") && d.Name == searchString) ||
                         ((selectField == "Author" || selectField == "All Fields") && d.Author == searchString) ||
                         ((selectField == "BinaryFile" || selectField == "All Fields") && d.BinaryFile == searchString)))
                    .OrderBy(sortBy, !reverse)
                    .ThenBy(d => d.Date, sortBy != "Date")
                    .ToList();
            }
        }

        public int CreateDocument(CreateDocModel createDocModel)
        {
            int id = 0;

            using (ISession session = OpenSession())
            {
                id = session.CreateSQLQuery($@"exec docInsert :Name, :BinaryFile")
                    .SetString("Name", createDocModel.Name)
                    .SetString("BinaryFile", createDocModel.BinaryFile.FileName)
                    .List<int>().First();
                return id;
            }
        }

        public Documents GetDocumentByID(int id)
        {
            using (var session = OpenSession())
            {
                return session.Get<Documents>(id);
            }
        }
    }
}