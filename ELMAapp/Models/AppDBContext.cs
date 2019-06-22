using System.Data.Entity;

namespace ELMAapp.Models
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public DbSet<Document> Documents { get; set; }
    }
}