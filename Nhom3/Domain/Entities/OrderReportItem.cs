namespace Nhom3.Domain.Entities;

public class OrderReportItem
{
    public int Id { get; set; }
    public int OrderReportId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public OrderReport? OrderReport { get; set; }
}
