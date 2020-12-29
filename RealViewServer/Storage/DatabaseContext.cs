using Microsoft.EntityFrameworkCore;
using RealViewServer.Model;

namespace RealViewServer.Storage
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Project> Projects { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Storage.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<Project>()
                .HasKey(p => p.Id);
        }
    }
}
