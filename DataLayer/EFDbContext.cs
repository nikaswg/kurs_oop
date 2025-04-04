using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class EFDbContext : DbContext
    {
        public EFDbContext(DbContextOptions<EFDbContext> options)
        : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Book_Issuance> Book_Issuances { get; set; }
        public DbSet<Reader> Readers { get; set; }
        public DbSet<Staff> Staff {  get; set; }
        public DbSet<FavoriteBook> FavoriteBooks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка отношений, если нужно
            modelBuilder.Entity<FavoriteBook>()
                .HasIndex(f => new { f.UserName, f.ISBN })
                .IsUnique();
        }
    }

    public class EFDbContextFactory : IDesignTimeDbContextFactory<EFDbContext>
    {
        public EFDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EFDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=libDb;Trusted_Connection=True;MultipleActiveResultSets=true",
                b => b.MigrationsAssembly("DataLayer"));

            return new EFDbContext(optionsBuilder.Options);
        }
    }
}
