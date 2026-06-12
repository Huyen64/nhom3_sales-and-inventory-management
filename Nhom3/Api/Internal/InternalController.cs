using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nhom3.Application.DTOs;
using Nhom3.Application.Services;

namespace Nhom3.Api.Internal;

[ApiController]
[Route("api/internal")]
[AllowAnonymous]
public class InternalController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IReportService _reportService;
    private readonly IConfiguration _configuration;

    public InternalController(
        IUserService userService,
        IReportService reportService,
        IConfiguration configuration)
    {
        _userService = userService;
        _reportService = reportService;
        _configuration = configuration;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        if (!HasValidApiKey())
            return Unauthorized(new { success = false, message = "Internal API key không hợp lệ" });

        return Ok(new { success = true, data = await _userService.GetAllUsersAsync() });
    }

    [HttpGet("users/{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        if (!HasValidApiKey())
            return Unauthorized(new { success = false, message = "Internal API key không hợp lệ" });

        var user = await _userService.GetUserByIdAsync(id);
        return user is null
            ? NotFound(new { success = false, message = "Không tìm thấy User" })
            : Ok(new { success = true, data = user });
    }

    [HttpPost("order-events")]
    public async Task<IActionResult> ConsumeOrderEvent(OrderEventDto orderEvent)
    {
        if (!HasValidApiKey())
            return Unauthorized(new { success = false, message = "Internal API key không hợp lệ" });

        try
        {
            var applied = await _reportService.ConsumeOrderEventAsync(orderEvent);
            return Ok(new { success = true, applied });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    private bool HasValidApiKey()
    {
        var expected = _configuration["Services:InternalApiKey"];
        var actual = Request.Headers["X-Internal-Api-Key"].ToString();
        return !string.IsNullOrWhiteSpace(expected)
            && string.Equals(expected, actual, StringComparison.Ordinal);
    }
}
