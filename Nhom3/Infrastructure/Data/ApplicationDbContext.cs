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
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

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
                entity.Property(u => u.CustomerTier).HasDefaultValue("Regular");
                entity.Property(u => u.WorkStatus).HasDefaultValue("Active");
            });

            modelBuilder.Entity<AttendanceRecord>(entity =>
            {
                entity.ToTable("AttendanceRecords");
                entity.HasKey(value => value.Id);
                entity.HasIndex(value => new { value.UserId, value.WorkDate }).IsUnique();
                entity.Property(value => value.WorkDate).HasColumnType("date");
                entity.Property(value => value.HoursWorked).HasPrecision(6, 2);
                entity.HasOne(value => value.User)
                    .WithMany()
                    .HasForeignKey(value => value.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
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
