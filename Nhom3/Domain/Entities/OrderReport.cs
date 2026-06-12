namespace Nhom3.Domain.Entities;

public class OrderReport
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Guid LastEventId { get; set; }
    public string LastEventType { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal DebtAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public List<OrderReportItem> Items { get; set; } = new();
}
