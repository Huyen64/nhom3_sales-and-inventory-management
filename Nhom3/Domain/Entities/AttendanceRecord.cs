namespace Nhom3.Domain.Entities;

public class AttendanceRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime WorkDate { get; set; }
    public TimeSpan? CheckIn { get; set; }
    public TimeSpan? CheckOut { get; set; }
    public string Status { get; set; } = "Present";
    public decimal HoursWorked { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
    public User User { get; set; } = null!;
}
