using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhom3.Application.DTOs;
using Nhom3.Domain.Entities;
using Nhom3.Infrastructure.Data;
using UserEntity = Nhom3.Domain.Entities.User;

namespace Nhom3.Api.HumanResources;

[ApiController]
[Route("api/hr")]
public class HumanResourcesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HumanResourcesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("employees")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEmployees([FromQuery] string? search)
    {
        var query = _context.Users
            .AsNoTracking()
            .Where(user => user.Role != UserEntity.UserRole.Customer);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(user =>
                user.FullName.ToLower().Contains(term)
                || user.Email.ToLower().Contains(term)
                || user.UserName.ToLower().Contains(term));
        }

        var users = await query.OrderBy(user => user.FullName).ToListAsync();
        return Ok(new { success = true, data = users.Select(MapUser) });
    }

    [HttpGet("employees/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(value => value.Id == id && value.Role != UserEntity.UserRole.Customer);
        return user is null
            ? NotFound(new { success = false, message = "Không tìm thấy nhân sự" })
            : Ok(new { success = true, data = MapUser(user) });
    }

    [HttpGet("employees/{id:int}/attendance")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEmployeeAttendance(int id)
    {
        var records = await _context.AttendanceRecords
            .AsNoTracking()
            .Where(value => value.UserId == id)
            .OrderByDescending(value => value.WorkDate)
            .Select(value => MapAttendance(value))
            .ToListAsync();
        return Ok(new { success = true, data = records });
    }

    [HttpGet("me/attendance")]
    [Authorize]
    public async Task<IActionResult> GetMyAttendance()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized(new { success = false, message = "Token không hợp lệ" });

        var records = await _context.AttendanceRecords
            .AsNoTracking()
            .Where(value => value.UserId == userId.Value)
            .OrderByDescending(value => value.WorkDate)
            .Select(value => MapAttendance(value))
            .ToListAsync();
        return Ok(new { success = true, data = records });
    }

    [HttpPost("attendance")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpsertAttendance(UpsertAttendanceDto dto)
    {
        if (dto.UserId <= 0)
            return BadRequest(new { success = false, message = "UserId không hợp lệ" });

        var userExists = await _context.Users.AnyAsync(user =>
            user.Id == dto.UserId && user.Role != UserEntity.UserRole.Customer);
        if (!userExists)
            return NotFound(new { success = false, message = "Không tìm thấy nhân sự" });

        var workDate = dto.WorkDate.Date;
        var record = await _context.AttendanceRecords
            .FirstOrDefaultAsync(value => value.UserId == dto.UserId && value.WorkDate == workDate);

        if (record is null)
        {
            record = new AttendanceRecord { UserId = dto.UserId, WorkDate = workDate };
            _context.AttendanceRecords.Add(record);
        }

        record.CheckIn = dto.CheckIn;
        record.CheckOut = dto.CheckOut;
        record.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Present" : dto.Status.Trim();
        record.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
        record.HoursWorked = CalculateHours(dto.CheckIn, dto.CheckOut);
        record.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { success = true, data = MapAttendance(record) });
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private static decimal CalculateHours(TimeSpan? checkIn, TimeSpan? checkOut)
    {
        if (!checkIn.HasValue || !checkOut.HasValue || checkOut <= checkIn)
            return 0;
        return Math.Round((decimal)(checkOut.Value - checkIn.Value).TotalHours, 2);
    }

    private static UserResponseDto MapUser(UserEntity user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role,
        DateOfBirth = user.DateOfBirth,
        Sex = user.Sex,
        Address = user.Address,
        PaidOrderCount = user.PaidOrderCount,
        CustomerTier = user.CustomerTier,
        CustomerTierLabel = user.CustomerTier switch
        {
            "Diamond" => "Kim cương",
            "Gold" => "Vàng",
            "Silver" => "Bạc",
            _ => "Thành viên thường"
        },
        WorkStatus = user.WorkStatus,
        CreatedAt = user.CreatedAt,
        LastModified = user.LastModified
    };

    private static AttendanceRecordDto MapAttendance(AttendanceRecord record) => new()
    {
        Id = record.Id,
        UserId = record.UserId,
        WorkDate = record.WorkDate,
        CheckIn = record.CheckIn,
        CheckOut = record.CheckOut,
        Status = record.Status,
        HoursWorked = record.HoursWorked,
        Note = record.Note
    };
}
