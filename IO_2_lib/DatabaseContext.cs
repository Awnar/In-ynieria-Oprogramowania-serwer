using IO_2_lib.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace IO_2_lib
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<AuthKey> AuthKeys { get; set; }

        public DatabaseContext() : base() { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseLazyLoadingProxies().UseSqlite("Data Source=ServerDB.db");
    }
}
