using DapperEFCorePostgreSQL.Entities;
using DapperEFCorePostgreSQL.ValueGenerators;
using Microsoft.EntityFrameworkCore;

namespace DapperEFCorePostgreSQL.Context
{
    public class SampleDbContext : DbContext
    {
        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<Category> Categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Constants.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(pk => pk.CategoryId);
                //entity.HasIndex(i => i.CreateDate);
                entity.Property(p => p.CategoryName).IsRequired().HasMaxLength(40);
                entity.Property(p => p.CreateDate).HasValueGenerator<DateTimeGenerator>().ValueGeneratedOnAdd();
                entity.HasMany(m => m.Products).WithOne(o => o.Category).HasForeignKey(fk => fk.CategoryId);
            });
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(pk => pk.ProductId);
                //entity.HasIndex(i => i.Name);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(40);
                entity.Property(p => p.Description).IsRequired().HasMaxLength(240);
                entity.Property(p => p.CreateDate).HasValueGenerator<DateTimeGenerator>().ValueGeneratedOnAdd();
                entity.HasOne(o => o.Category).WithMany(m => m.Products).HasForeignKey(fk => fk.CategoryId);
            });
        }
    }
}
