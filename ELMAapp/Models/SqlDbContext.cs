using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ELMAapp.Models
{
    public static class SqlDbContext
    {
        public static AppDBContext CreateAdmin()
        {
            return new AppDBContext("AdminElma");
        }

        private static List<string> owners;
        private static string password = "BadCodeForever";
        private static string database = "APPELMA_390bf6b01fa044f1ba015ff8693ef461";

        public static void AddDbUser(string userName)
        {
            AddDbUser(userName, password);
        }

        public static void AddDbUser(string userName, string password)
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
            var adm = CreateAdmin();
            adm.Database.ExecuteSqlCommand(sql);
            adm.Dispose();
            UpdateOwners();
            if (!owners.Contains(userName))
                throw new Exception($@"failed to create database user : {userName}");
        }

        public static AppDBContext CreateUserContext(string userName)
        {
            return CreateUserContext(userName, password);
        }

        public static AppDBContext CreateUserContext(string userName, string password)
        {
            if (owners == null)
                UpdateOwners();

            if (owners.Contains(userName))
                return new AppDBContext(
                    $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AppELMA.mdf;User Id={userName};Password={password};");

            AddDbUser(userName, password);
            return new AppDBContext(
                $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AppELMA.mdf;User Id={userName};Password={password};");
        }

        private static void UpdateOwners()
        {
            var adm = CreateAdmin();
            owners = adm.Database.SqlQuery<string>(@"select name from master.sys.server_principals")
                .ToList();
            adm.Dispose();
        }
    }
}