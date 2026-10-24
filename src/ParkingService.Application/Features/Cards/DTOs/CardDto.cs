using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Cards.DTOs;

/// <summary>
/// DTO chứa thông tin thẻ gửi xe và người dùng liên kết (nếu có).
/// Giúp dễ dàng kiểm thử và validate quyền ra/vào dựa trên CardNumber và số xe.
/// </summary>
public class CardDto
{
    /// <summary>
    /// Mã định danh duy nhất của thẻ (UUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Mã số in hoặc đọc từ thẻ (RFID / Mifare UID).
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// ID của người dùng được gán thẻ (null nếu là thẻ chưa gán hoặc thẻ khách).
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Phân loại thẻ (1 = Registered, 2 = Temporary, 3 = Unknown).
    /// </summary>
    public CardType Type { get; set; } = CardType.Registered;

    /// <summary>
    /// Trạng thái hoạt động của thẻ (1 = Active, 2 = Blocked, 3 = Lost).
    /// </summary>
    public CardStatus Status { get; set; } = CardStatus.Active;

    /// <summary>
    /// Cờ bỏ qua kiểm tra khớp biển số xe khi quẹt thẻ ra/vào.
    /// Mặc định là false (phải kiểm tra khớp biển số). Nếu true, cho phép xe qua mà không cần khớp biển số đăng ký.
    /// </summary>
    public bool SkipValidatePlate { get; set; } = false;

    // --- Thông tin liên kết từ User (dùng để validate số xe và kiểm thử) ---

    /// <summary>
    /// Họ và tên người dùng sở hữu thẻ.
    /// </summary>
    public string? UserFullName { get; set; }

    /// <summary>
    /// Biển số xe đăng ký của người dùng (dùng để đối chiếu với ảnh chụp OCR cổng vào/ra).
    /// </summary>
    public string? UserPlateNumber { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ của người dùng.
    /// </summary>
    public string? UserPhone { get; set; }

    /// <summary>
    /// Số phòng / căn hộ của người dùng.
    /// </summary>
    public string? UserRoom { get; set; }

    /// <summary>
    /// Trạng thái của người dùng (1 = Active, 2 = Disabled).
    /// </summary>
    public UserStatus? UserStatus { get; set; }

    /// <summary>
    /// Thời gian cấp thẻ (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời gian cập nhật thẻ gần nhất (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
