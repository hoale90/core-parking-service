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
    /// Loại thẻ gửi xe (1 = Registered / Thẻ đăng ký, 2 = Temporary / Thẻ tạm, 3 = Unknown / Chưa xác định).
    /// </summary>
    public CardType Type { get; set; } = CardType.Registered;

    /// <summary>
    /// Biển số xe gắn với thẻ (ví dụ: "59-X1 123.45", "38B1-28228").
    /// </summary>
    public string? VehiclePlate { get; set; }

    /// <summary>
    /// Cờ bỏ qua xác thực nhận diện biển số xe (true = cho phép quẹt thẻ qua không cần khớp biển số).
    /// </summary>
    public bool SkipValidateVehiclePlate { get; set; }

    /// <summary>
    /// Thông tin bổ sung, họ tên chủ thẻ hoặc ghi chú (ví dụ: "ĐINH HỮU ĐỨC", "MẤT THẺ").
    /// </summary>
    public string? Info { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ của chủ thẻ.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Số phòng / căn hộ gắn với thẻ (ví dụ: "B22", "A16").
    /// </summary>
    public string? Room { get; set; }

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
