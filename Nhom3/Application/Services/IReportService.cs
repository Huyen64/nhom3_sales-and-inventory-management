using Nhom3.Application.DTOs;

namespace Nhom3.Application.Services;

public interface IReportService
{
    Task<bool> ConsumeOrderEventAsync(OrderEventDto orderEvent);
    Task<DashboardReportDto> GetDashboardAsync();
    Task<RevenueChartDto> GetRevenueChartAsync(string groupBy, DateTime? from, DateTime? to);
}
