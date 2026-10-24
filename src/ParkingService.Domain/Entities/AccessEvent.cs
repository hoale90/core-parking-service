using ParkingService.Domain.Common;
using ParkingService.Domain.Enums;

namespace ParkingService.Domain.Entities;

/// <summary>
/// Thực thể ghi nhận lịch sử từng lượt quẹt thẻ / tương tác qua cổng kiểm soát xe.
/// Bảng audit log không bao giờ bị xóa, ghi nhận đầy đủ cả lượt thành công và bị từ chối.
/// </summary>
public class AccessEvent : BaseEntity
{
    /// <summary>
    /// Mã số thẻ RFID/Mifare quẹt tại đầu đọc.
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Khóa ngoại liên kết tới thẻ trong bảng Cards (nếu tìm thấy).
    /// </summary>
    public Guid? CardId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới người dùng sở hữu thẻ (nếu thẻ đã gán cho User).
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Hướng quẹt thẻ (In = Vào, Out = Ra).
    /// </summary>
    public AccessDirection Direction { get; set; } = AccessDirection.In;

    /// <summary>
    /// Mã làn xe xảy ra sự kiện (ví dụ: "LANE_IN_01", "LANE_OUT_01").
    /// </summary>
    public string? LaneCode { get; set; }

    /// <summary>
    /// Biển số xe do OCR Service nhận diện được từ ảnh chụp camera.
    /// </summary>
    public string? OcrPlateNumber { get; set; }

    /// <summary>
    /// Biển số xe sau khi được chuẩn hóa (loại bỏ dấu gạch, dấu chấm, khoảng trắng, viết hoa).
    /// </summary>
    public string? PlateNormalized { get; set; }

    /// <summary>
    /// Biển số xe đăng ký của người dùng sở hữu thẻ (dùng để đối chiếu).
    /// </summary>
    public string? ExpectedPlateNumber { get; set; }

    /// <summary>
    /// Tỷ lệ phần trăm trùng khớp giữa biển số OCR nhận diện và biển số đăng ký (0.0% -> 100.0%).
    /// </summary>
    public double? PlateSimilarityPercent { get; set; }

    /// <summary>
    /// Cờ bỏ qua kiểm tra biển số xe tại thời điểm quẹt (lấy từ cấu hình thẻ).
    /// </summary>
    public bool SkipValidatePlate { get; set; } = false;

    /// <summary>
    /// Kết quả quyết định: Allowed (Cho phép qua) hoặc Denied (Từ chối).
    /// </summary>
    public AccessDecision Decision { get; set; } = AccessDecision.Denied;

    /// <summary>
    /// Mã lý do quyết định (ví dụ: VALID, SKIP_VALIDATE_ALLOWED, CARD_NOT_FOUND, CARD_INACTIVE, USER_DISABLED, PLATE_MISMATCH, NO_USER_ASSIGNED).
    /// </summary>
    public string ReasonCode { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn tương đối lưu trữ file ảnh chụp xe tại camera khi quẹt thẻ.
    /// </summary>
    public string? SnapshotPath { get; set; }

    /// <summary>
    /// Thời điểm quẹt thẻ thực tế (UTC).
    /// </summary>
    public DateTime EventTime { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Card? Card { get; set; }
    public User? User { get; set; }
}
