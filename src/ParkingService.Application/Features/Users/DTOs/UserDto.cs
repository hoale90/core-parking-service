using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Users.DTOs;

/// <summary>
/// DTO chứa thông tin chi tiết người dùng gửi xe / nhân viên / cư dân.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Mã định danh duy nhất của người dùng (UUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Họ và tên đầy đủ.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Tên tài khoản người dùng / nhân viên (username).
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// ID vai trò người dùng (1: Technical Admin, 3: Admin, 4: Staff,...).
    /// </summary>
    public int? RoleId { get; set; }

    /// <summary>
    /// Tên vai trò người dùng (ví dụ: "admin", "staff", "resident").
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Số phòng / căn hộ cư dân (ví dụ: "B22", "A16").
    /// </summary>
    public string? Room { get; set; }

    /// <summary>
    /// Ngày sinh (UTC).
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Giới tính.
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Số điện thoại liên lạc.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Địa chỉ email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Biển số xe đăng ký (ví dụ: "59-X1 123.45").
    /// </summary>
    public string? PlateNumber { get; set; }

    /// <summary>
    /// Trạng thái tài khoản (1 = Active, 2 = Disabled).
    /// </summary>
    public UserStatus Status { get; set; }

    /// <summary>
    /// Thời gian tạo bản ghi (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời gian cập nhật gần nhất (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
