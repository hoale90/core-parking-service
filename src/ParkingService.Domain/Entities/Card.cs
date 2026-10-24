using ParkingService.Domain.Common;
using ParkingService.Domain.Enums;

namespace ParkingService.Domain.Entities;

/// <summary>
/// Bảng Card: Chứa thông tin mã số thẻ RFID/Mifare và trạng thái thẻ.
/// Có liên kết UserId (nullable) để gán thẻ cho người dùng.
/// </summary>
public class Card : BaseEntity
{
    public string CardNumber { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public CardType Type { get; set; } = CardType.Registered;
    public CardStatus Status { get; set; } = CardStatus.Active;

    /// <summary>
    /// Cờ bỏ qua kiểm tra khớp biển số xe khi quẹt thẻ ra/vào.
    /// Mặc định là false (phải kiểm tra khớp biển số). Nếu true, cho phép xe qua mà không cần khớp biển số đăng ký.
    /// </summary>
    public bool SkipValidatePlate { get; set; } = false;

    // Navigation property: Người dùng sở hữu thẻ
    public User? User { get; set; }
}
