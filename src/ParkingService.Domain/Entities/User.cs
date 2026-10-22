using ParkingService.Domain.Common;
using ParkingService.Domain.Enums;

namespace ParkingService.Domain.Entities;

/// <summary>
/// Bảng User: Chứa thông tin cá nhân của người dùng / cư dân gửi xe.
/// Quản lý họ tên, số điện thoại, số phòng và số xe để đối chiếu khi ra/vào.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? PlateNumber { get; set; }
    public string? Room { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;

    // Navigation property: Danh sách thẻ liên kết với người dùng
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
