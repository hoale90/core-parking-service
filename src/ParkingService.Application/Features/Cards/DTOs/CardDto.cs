using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Cards.DTOs;

/// <summary>
/// DTO chứa thông tin chi tiết thẻ gửi xe.
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
    /// ID của người dùng sở hữu thẻ (null nếu là thẻ khách hoặc chưa gán).
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của thẻ (1 = Active, 2 = Blocked, 3 = Lost).
    /// </summary>
    public CardStatus Status { get; set; }

    /// <summary>
    /// Thời gian cấp thẻ (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời gian cập nhật thẻ gần nhất (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
