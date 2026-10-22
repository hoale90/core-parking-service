using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Users.DTOs;

/// <summary>
/// DTO chứa thông tin chi tiết người dùng / cư dân gửi xe.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Mã định danh duy nhất của người dùng (UUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Họ và tên đầy đủ của người dùng.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại liên lạc.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Địa chỉ email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Biển số xe đăng ký của người dùng (dùng để đối chiếu khi quẹt thẻ qua cổng).
    /// </summary>
    public string? PlateNumber { get; set; }

    /// <summary>
    /// Số phòng / căn hộ cư dân (ví dụ: "A16", "B22").
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
    /// Trạng thái hoạt động của người dùng (1 = Active, 2 = Disabled).
    /// </summary>
    public UserStatus Status { get; set; }

    /// <summary>
    /// Mã số thẻ đang được gán cho người dùng (nếu có).
    /// </summary>
    public string? LinkedCardNumber { get; set; }

    /// <summary>
    /// Thời gian tạo bản ghi (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời gian cập nhật gần nhất (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
