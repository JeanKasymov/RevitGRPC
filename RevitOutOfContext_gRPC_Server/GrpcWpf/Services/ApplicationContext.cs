using GrpcWpf.Models;
using Microsoft.EntityFrameworkCore;
namespace GrpcWpf.Services
{
    public class ApplicationContext : DbContext
    { 
        public DbSet<Family> families { get; set; } = null!;
        public DbSet<Plugin> plugins { get; set; } = null!;

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)                                   
        {
            optionsBuilder.UseMySql("server=;user=;password=d;database=sb;", new MySqlServerVersion(new Version(8,0,30)));
        }
    }
}