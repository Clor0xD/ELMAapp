using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using Microsoft.Web.Mvc;
using WebMatrix.WebData;

namespace ELMAapp.Models
{
    public static class CloudContext // может стоило с конфига сюда грузить но простили простейший пример файлового хранилища.
    {
        private static List<string> _owners;

        private const string Database = @"C:\USERS\USER\DOCUMENTS\VISUAL STUDIO 2017\PROJECTS\ELMAAPP\ELMAAPP\APP_DATA\APPELMA.MDF";


        public static AppDBContext
            CreateDbContext() // connection state не работает до EF6 капец баги на багах в этом MVC4 + EF4
        {
            var uc = new UsersContext();
            var currentUser = Membership.GetUser();
            if (currentUser == null)
                return null;
            var db = CreateUserContext(currentUser.UserName,
                // ReSharper disable once ReplaceWithSingleCallToFirst fist don't sql transform
                uc.UserProfiles.Where(up => up.UserId == WebSecurity.CurrentUserId).First()
                    .SqlPassword);
            uc.Dispose();
            return db;
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
                , userName, password, Database);
            var adm = new AppDBContext("AdminElma");
            adm.Database.ExecuteSqlCommand(sql);
            adm.Dispose();
            UpdateOwners();
            if (!_owners.Contains(userName))
                throw new Exception($@"failed to create database user : {userName}");
        }

        private static AppDBContext CreateUserContext(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return null;
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