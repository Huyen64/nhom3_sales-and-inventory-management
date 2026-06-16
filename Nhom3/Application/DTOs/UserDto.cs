using System;
using Nhom3.Domain.Entities;

namespace Nhom3.Application.DTOs
{
    public class CreateUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public User.UserRole Role { get; set; } = User.UserRole.SalesStaff;
        public User.Gender Sex { get; set; }
        public string Address { get; set; } = string.Empty;
    }

    public class RegisterCustomerDto
    {
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public User.Gender Sex { get; set; }
        public string Address { get; set; } = string.Empty;
    }

    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public User.UserRole? Role { get; set; }
        public User.Gender? Sex { get; set; }
        public string? Address { get; set; }
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public User.UserRole Role { get; set; }
        public DateTime DateOfBirth { get; set; }
        public User.Gender Sex { get; set; }
        public string Address { get; set; } = string.Empty;
        public int PaidOrderCount { get; set; }
        public string CustomerTier { get; set; } = "Regular";
        public string CustomerTierLabel { get; set; } = "Thành viên thường";
        public string WorkStatus { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
    }
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public UserResponseDto User { get; set; } = new();
    }

    public class RefreshRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    public class LogoutRequestDto
    {
        public string? RefreshToken { get; set; }
        public string? DeviceId { get; set; }
    }

    public class CustomerMembershipDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public int PaidOrderCount { get; set; }
        public string Tier { get; set; } = "Regular";
        public string TierLabel { get; set; } = "Thành viên thường";
        public decimal DiscountPercent { get; set; }
    }

    public class PaidOrderAppliedDto
    {
        public string Email { get; set; } = string.Empty;
        public int OrderId { get; set; }
    }

    public class AttendanceRecordDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime WorkDate { get; set; }
        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal HoursWorked { get; set; }
        public string? Note { get; set; }
    }

    public class UpsertAttendanceDto
    {
        public int UserId { get; set; }
        public DateTime WorkDate { get; set; }
        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }
        public string Status { get; set; } = "Present";
        public string? Note { get; set; }
    }
}
