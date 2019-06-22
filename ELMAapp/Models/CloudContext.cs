using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Web.Security;
using WebMatrix.WebData;

namespace ELMAapp.Models
{
    public static class CloudContext
    {
        private static List<string> _owners;
        private static string database = @"C:\USERS\USER\DOCUMENTS\VISUAL STUDIO 2017\PROJECTS\ELMAAPP\ELMAAPP\APP_DATA\APPELMA.MDF";
        private static UsersContext _userContext;
        private static UsersContext UserContext
        {
            get
            {
                if (_userContext == null || (_userContext.Database.Connection.State != ConnectionState.Open))
                {
                    _userContext = new UsersContext();
                }

                return _userContext;
            }
        }

        private static AppDBContext _db;

        public static AppDBContext DB
        {
            get
            {
                if (_db == null || (_db.Database.Connection.State != ConnectionState.Open))
                    _db = CreateUserContext(Membership.GetUser().UserName,
                        UserContext.UserProfiles.Where(up => up.UserId == WebSecurity.CurrentUserId).First()
                            .SqlPassword);
                return _db;
            }
        }

        private static void AddDbUser(string userName, string password)
        {
            var sql = string.Format(@"
            USE[master]
            CREATE LOGIN[{0}] WITH PASSWORD = N'{1}', DEFAULT_DATABASE =[master], CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF
            ALTER SERVER ROLE [dbcreator] ADD MEMBER [{0}]
            USE[{2}]
            CREATE USER[{0}] FOR LOGIN[{0}]
            ALTER USER[{0}] WITH DEFAULT_SCHEMA =[db_owner]
            ALTER ROLE[db_owner] ADD MEMBER[{0}]"
                , userName, password, database);
            var adm = new AppDBContext("AdminElma");
            adm.Database.ExecuteSqlCommand(sql);
            adm.Dispose();
            UpdateOwners();
            if (!_owners.Contains(userName))
                throw new Exception($@"failed to create database user : {userName}");
        }

        private static AppDBContext CreateUserContext(string userName, string password)
        {
            if (_owners == null)
                UpdateOwners();

            if (_owners.Contains(userName))
                return new AppDBContext(
                    $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AppELMA.mdf;User Id={userName};Password={password};");

            AddDbUser(userName, password);
            return new AppDBContext(
                $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AppELMA.mdf;User Id={userName};Password={password};");
        }

        private static void UpdateOwners()
        {
            var adm = new AppDBContext("AdminElma");
            _owners = adm.Database.SqlQuery<string>(@"select name from master.sys.server_principals")
                .ToList();
            adm.Dispose();
        }
    }
}