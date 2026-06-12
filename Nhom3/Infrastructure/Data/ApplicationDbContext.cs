using Microsoft.EntityFrameworkCore;
using Nhom3.Domain.Entities;

namespace Nhom3.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<BlacklistedToken> BlacklistedTokens { get; set; }
        public DbSet<OrderReport> OrderReports { get; set; }
        public DbSet<OrderReportItem> OrderReportItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasDefaultValue(User.UserRole.SalesStaff);
                entity.Property(u => u.DateOfBirth).HasColumnType("date");
            });

            modelBuilder.Entity<BlacklistedToken>(entity =>
            {
                entity.ToTable("BlacklistedTokens");
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.Jti).IsUnique();
                entity.HasIndex(t => t.ExpiresAt);
            });

            modelBuilder.Entity<OrderReport>(entity =>
            {
                entity.ToTable("OrderReports");
                entity.HasKey(report => report.Id);
                entity.HasIndex(report => report.OrderId).IsUnique();
                entity.HasIndex(report => report.CreatedAt);
                entity.Property(report => report.Subtotal).HasPrecision(18, 2);
                entity.Property(report => report.DiscountAmount).HasPrecision(18, 2);
                entity.Property(report => report.TotalAmount).HasPrecision(18, 2);
                entity.Property(report => report.AmountPaid).HasPrecision(18, 2);
                entity.Property(report => report.DebtAmount).HasPrecision(18, 2);
                entity.HasMany(report => report.Items)
                    .WithOne(item => item.OrderReport)
                    .HasForeignKey(item => item.OrderReportId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderReportItem>(entity =>
            {
                entity.ToTable("OrderReportItems");
                entity.HasKey(item => item.Id);
                entity.HasIndex(item => item.ProductId);
                entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
                entity.Property(item => item.Subtotal).HasPrecision(18, 2);
            });
        }
    }
}
