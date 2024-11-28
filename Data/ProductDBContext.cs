using CodeTest.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeTest.Data
{
    public class ProductDBContext : DbContext
    {
        public ProductDBContext()
        {
        }

        public ProductDBContext(DbContextOptions<ProductDBContext> opt) : base(opt)
        {

        }

        public DbSet<Product> Products { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //-> for postgresql as

                // optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=CodeTest;Username=postgres;Password=123;");

                //-> for sql server 
                optionsBuilder.UseSqlServer("");

            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.ListPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.MinDiscount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.DiscountPrice)
                .HasPrecision(18, 2); 
        }

    }
}