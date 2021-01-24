using IO_2_lib.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.Reflection;

namespace IO_2_lib
{
    public class DatabaseContext : DbContext
    {
        public static bool IsInMemory = false;

        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<AuthKey> AuthKeys { get; set; }

        public DatabaseContext() : base() { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if(IsInMemory)
            {
                //options.UseLazyLoadingProxies().UseSqlite("Data Source=:memory:");
                options.UseLazyLoadingProxies().UseSqlite(CreateInMemoryDatabase());
            }
            else
            {
                options.UseLazyLoadingProxies().UseSqlite("Data Source=ServerDB.db");
            }
        }


        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");

            connection.Open();

            return connection;
        }
    }
}
