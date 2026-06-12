using Microsoft.EntityFrameworkCore;
using Nhom3.Application.DTOs;
using Nhom3.Domain.Entities;
using Nhom3.Infrastructure.Data;

namespace Nhom3.Application.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ConsumeOrderEventAsync(OrderEventDto orderEvent)
    {
        if (orderEvent.EventId == Guid.Empty)
            throw new ArgumentException("EventId không hợp lệ");

        if (orderEvent.OrderId <= 0)
            throw new ArgumentException("OrderId không hợp lệ");

        if (string.IsNullOrWhiteSpace(orderEvent.EventType))
            throw new ArgumentException("EventType không được để trống");

        var occurredAt = orderEvent.OccurredAt == default
            ? DateTime.UtcNow
            : orderEvent.OccurredAt;
        var report = await _context.OrderReports
            .Include(value => value.Items)
            .FirstOrDefaultAsync(value => value.OrderId == orderEvent.OrderId);

        if (report is not null
            && (report.LastEventId == orderEvent.EventId || report.LastUpdatedAt > occurredAt))
        {
            return false;
        }

        if (report is null)
        {
            report = new OrderReport
            {
                OrderId = orderEvent.OrderId
            };
            await _context.OrderReports.AddAsync(report);
        }
        else
        {
            _context.OrderReportItems.RemoveRange(report.Items);
            report.Items.Clear();
        }

        report.LastEventId = orderEvent.EventId;
        report.LastEventType = orderEvent.EventType;
        report.UserId = orderEvent.UserId;
        report.CustomerId = orderEvent.CustomerId;
        report.CustomerName = orderEvent.CustomerName;
        report.Status = orderEvent.Status;
        report.Subtotal = orderEvent.Subtotal;
        report.DiscountAmount = orderEvent.DiscountAmount;
        report.TotalAmount = orderEvent.TotalAmount;
        report.AmountPaid = orderEvent.AmountPaid;
        report.DebtAmount = orderEvent.DebtAmount;
        report.CreatedAt = orderEvent.CreatedAt;
        report.LastUpdatedAt = occurredAt;
        report.Items = orderEvent.Items.Select(item => new OrderReportItem
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            Subtotal = item.Subtotal
        }).ToList();

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<DashboardReportDto> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekStart = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var orders = await GetActiveOrdersAsync();

        var topProducts = orders
            .SelectMany(order => order.Items)
            .GroupBy(item => new { item.ProductId, item.ProductName })
            .Select(group => new TopProductDto
            {
                ProductId = group.Key.ProductId,
                ProductName = group.Key.ProductName,
                QuantitySold = group.Sum(item => item.Quantity),
                Revenue = group.Sum(item => item.Subtotal)
            })
            .OrderByDescending(item => item.QuantitySold)
            .ThenByDescending(item => item.Revenue)
            .Take(10)
            .ToList();

        var topCustomers = orders
            .Where(order => order.CustomerId.HasValue)
            .GroupBy(order => new { order.CustomerId, order.CustomerName })
            .Select(group => new TopCustomerDto
            {
                CustomerId = group.Key.CustomerId,
                CustomerName = group.Key.CustomerName ?? "Khách hàng",
                OrderCount = group.Count(),
                Revenue = group.Sum(order => order.TotalAmount),
                Debt = group.Sum(order => order.DebtAmount)
            })
            .OrderByDescending(customer => customer.Revenue)
            .Take(10)
            .ToList();

        return new DashboardReportDto
        {
            RevenueToday = orders
                .Where(order => order.CreatedAt >= today)
                .Sum(order => order.TotalAmount),
            RevenueThisWeek = orders
                .Where(order => order.CreatedAt >= weekStart)
                .Sum(order => order.TotalAmount),
            RevenueThisMonth = orders
                .Where(order => order.CreatedAt >= monthStart)
                .Sum(order => order.TotalAmount),
            OrderCount = orders.Count,
            TopProducts = topProducts,
            TopCustomers = topCustomers
        };
    }

    public async Task<RevenueChartDto> GetRevenueChartAsync(
        string groupBy,
        DateTime? from,
        DateTime? to)
    {
        var normalizedGroup = groupBy.Trim().ToLowerInvariant();
        if (normalizedGroup is not ("day" or "month"))
            throw new ArgumentException("groupBy chỉ nhận 'day' hoặc 'month'");

        var end = (to ?? DateTime.UtcNow).Date.AddDays(1);
        var start = (from ?? (normalizedGroup == "day"
            ? end.AddDays(-30)
            : end.AddMonths(-12))).Date;

        if (start >= end)
            throw new ArgumentException("Khoảng thời gian báo cáo không hợp lệ");

        var orders = (await GetActiveOrdersAsync())
            .Where(order => order.CreatedAt >= start && order.CreatedAt < end)
            .ToList();

        var points = normalizedGroup == "day"
            ? orders
                .GroupBy(order => order.CreatedAt.Date)
                .Select(group => new ChartPoint(
                    group.Key,
                    group.Sum(order => order.TotalAmount),
                    group.Count()))
                .OrderBy(point => point.Date)
                .ToList()
            : orders
                .GroupBy(order => new DateTime(
                    order.CreatedAt.Year,
                    order.CreatedAt.Month,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc))
                .Select(group => new ChartPoint(
                    group.Key,
                    group.Sum(order => order.TotalAmount),
                    group.Count()))
                .OrderBy(point => point.Date)
                .ToList();

        return new RevenueChartDto
        {
            GroupBy = normalizedGroup,
            From = start,
            To = end.AddTicks(-1),
            Labels = points
                .Select(point => point.Date.ToString(normalizedGroup == "day" ? "yyyy-MM-dd" : "yyyy-MM"))
                .ToList(),
            Revenue = points.Select(point => point.Revenue).ToList(),
            OrderCount = points.Select(point => point.OrderCount).ToList()
        };
    }

    private Task<List<OrderReport>> GetActiveOrdersAsync()
    {
        return _context.OrderReports
            .AsNoTracking()
            .Include(report => report.Items)
            .Where(report => report.Status != "Cancelled")
            .ToListAsync();
    }

    private sealed record ChartPoint(DateTime Date, decimal Revenue, int OrderCount);
}
