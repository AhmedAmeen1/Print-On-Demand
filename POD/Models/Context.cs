using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace POD.Models
{
    public class Context : IdentityDbContext<ApplicationUser>
    {
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<SellerProfile> SellerProfiles { get; set; }
        public DbSet<ProductTemplate> ProductTemplates { get; set; }
        public DbSet<CustomProduct> CustomProducts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasMany(u => u.Orders)
                      .WithOne(o => o.User)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.SellerProfile)
                      .WithOne(s => s.User)
                      .HasForeignKey<SellerProfile>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.CustomProducts)
                      .WithOne(cp => cp.User)
                      .HasForeignKey(cp => cp.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.CartItems)
                      .WithOne(ci => ci.User)
                      .HasForeignKey(ci => ci.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Order>(entity =>
            {
                entity.HasMany(o => o.Payments)
                      .WithOne(p => p.Order)
                      .HasForeignKey(p => p.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(o => o.OrderItems)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<SellerProfile>(entity =>
            {
                entity.HasMany(sp => sp.ProductTemplates)
                      .WithOne(pt => pt.SellerProfile)
                      .HasForeignKey(pt => pt.SellerProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ProductTemplate>(entity =>
            {
                entity.HasMany(pt => pt.CustomProducts)
                      .WithOne(cp => cp.ProductTemplate)
                      .HasForeignKey(cp => cp.ProductTemplateId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CustomProduct>(entity =>
            {
                entity.HasMany(cp => cp.CartItems)
                      .WithOne(ci => ci.CustomProduct)
                      .HasForeignKey(ci => ci.CustomProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(cp => cp.OrderItems)
                      .WithOne(oi => oi.CustomProduct)
                      .HasForeignKey(oi => oi.CustomProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure decimal precision
            builder.Entity<Order>()
                   .Property(o => o.TotalAmount)
                   .HasPrecision(18, 2);

            builder.Entity<Payment>()
                   .Property(p => p.Amount)
                   .HasPrecision(18, 2);

            builder.Entity<ProductTemplate>()
                   .Property(pt => pt.BasePrice)
                   .HasPrecision(18, 2);

            builder.Entity<CustomProduct>()
                   .Property(cp => cp.Price)
                   .HasPrecision(18, 2);

            builder.Entity<OrderItem>()
                   .Property(oi => oi.UnitPrice)
                   .HasPrecision(18, 2);

            builder.Entity<OrderItem>()
                   .Property(oi => oi.TotalPrice)
                   .HasPrecision(18, 2);
        }
    }
}
