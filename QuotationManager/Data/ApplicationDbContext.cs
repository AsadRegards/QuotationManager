using Microsoft.EntityFrameworkCore;
using QuotationManager.Models;

namespace QuotationManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default precision for all decimals globally (optional)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetPrecision(18);
                        property.SetScale(2);
                    }
                }
            }

            // OR define precision per property for clarity
            modelBuilder.Entity<Product>()
                .Property(p => p.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Quotation>()
                .Property(q => q.GSTPercentage)
                .HasPrecision(5, 2);

            modelBuilder.Entity<QuotationItem>(q =>
            {
                q.Property(x => x.UnitPrice).HasPrecision(18, 2);
                q.Property(x => x.TotalValue).HasPrecision(18, 2);
            });

            // Fix cascade conflict
            modelBuilder.Entity<QuotationItem>()
                .HasOne(q => q.Product)
                .WithMany()
                .HasForeignKey(q => q.ProductId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<QuotationItem>()
                .HasOne(q => q.Quotation)
                .WithMany(q => q.QuotationItems)
                .HasForeignKey(q => q.QuotationId)
                .OnDelete(DeleteBehavior.Cascade); // 👈 allow this one
        }


        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }
    }
}
