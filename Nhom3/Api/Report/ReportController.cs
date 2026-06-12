using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nhom3.Application.Services;

namespace Nhom3.Api.Report;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        return Ok(new { success = true, data = await _reportService.GetDashboardAsync() });
    }

    [HttpGet("revenue-chart")]
    public async Task<IActionResult> GetRevenueChart(
        [FromQuery] string groupBy = "day",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        try
        {
            var report = await _reportService.GetRevenueChartAsync(groupBy, from, to);
            return Ok(new { success = true, data = report });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
