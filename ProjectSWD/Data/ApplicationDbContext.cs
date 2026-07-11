using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data.Entities;

namespace ProjectSWD.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DeliveryPartner> DeliveryPartners { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Bill> Bills { get; set; }


        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionProduct> PromotionProducts { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<RefundItem> RefundItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Order>()
                .Property(o => o.ApprovementStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Shipment>()
                .Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Refund>()
                .Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(50);


            var decimal18_2_Props = new[]
            {
                typeof(Product).GetProperty("Price"),
                typeof(Order).GetProperty("TotalPrice"),
                typeof(OrderItem).GetProperty("Price"),
                typeof(Promotion).GetProperty("FixedAmount"),
                typeof(Promotion).GetProperty("MinimumOrder"),
                typeof(Refund).GetProperty("Amount"),
                typeof(RefundItem).GetProperty("Price")
            };

            foreach (var prop in decimal18_2_Props)
                modelBuilder.Entity(prop.DeclaringType).Property(prop.Name).HasColumnType("decimal(18, 2)");

            var decimal10_2_Props = new[]
            {
                typeof(Product).GetProperty("Quantity"),
                typeof(OrderItem).GetProperty("Quantity"),
                typeof(RefundItem).GetProperty("Quantity"),
                typeof(CartItem).GetProperty("Quantity")
            };

            foreach (var prop in decimal10_2_Props)
                modelBuilder.Entity(prop.DeclaringType).Property(prop.Name).HasColumnType("decimal(10, 2)");

            modelBuilder.Entity<Promotion>()
                .Property(p => p.Percentage).HasColumnType("decimal(5, 2)");


            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => new { oi.OrderId, oi.ProductId });

            modelBuilder.Entity<PromotionProduct>()
                .HasKey(pp => new { pp.PromotionId, pp.ProductId });

            modelBuilder.Entity<RefundItem>()
                .HasKey(ri => new { ri.RefundId, ri.ProductId });

            modelBuilder.Entity<CartItem>()
                .HasKey(ci => new { ci.CustomerId, ci.ProductId });

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.Quantity).HasColumnType("decimal(10, 2)");


            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer).WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Staff).WithMany(s => s.ManagedOrders)
                .HasForeignKey(o => o.StaffId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer).WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.OrderItem)
                .WithOne(oi => oi.Review)
                .HasForeignKey<Review>(r => new { r.OrderId, r.ProductId })
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.CustomerId, r.OrderId, r.ProductId })
                .IsUnique();
            modelBuilder.Entity<CartItem>()
                .HasKey(c => new { c.CustomerId, c.ProductId });

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Customer)
                .WithMany(cust => cust.CartItems)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}