using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Access.DTOs;

/// <summary>
/// Kết quả phản hồi sau khi xử lý quẹt thẻ ra/vào và đối chiếu biển số xe.
/// </summary>
public class SwipeCardResultDto
{
    /// <summary>
    /// Quyết định mở barrier: Allowed (Cho phép) hoặc Denied (Từ chối).
    /// </summary>
    public AccessDecision Decision { get; set; } = AccessDecision.Denied;

    /// <summary>
    /// Mã lý do (ví dụ: VALID, SKIP_VALIDATE_ALLOWED, CARD_NOT_FOUND, CARD_INACTIVE, USER_DISABLED, PLATE_MISMATCH, NO_USER_ASSIGNED).
    /// </summary>
    public string ReasonCode { get; set; } = string.Empty;

    /// <summary>
    /// Thông điệp diễn giải lý do bằng tiếng Việt để hiển thị lên màn hình trạm kiểm soát.
    /// </summary>
    public string ReasonMessage { get; set; } = string.Empty;

    /// <summary>
    /// Mã số thẻ quẹt.
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Hướng quẹt thẻ (In / Out).
    /// </summary>
    public AccessDirection Direction { get; set; }

    /// <summary>
    /// Mã làn kiểm soát (ví dụ: LANE_IN_01).
    /// </summary>
    public string? LaneCode { get; set; }

    /// <summary>
    /// Biển số xe do camera/OCR nhận diện được.
    /// </summary>
    public string? DetectedPlate { get; set; }

    /// <summary>
    /// Biển số xe đăng ký của người dùng sở hữu thẻ.
    /// </summary>
    public string? ExpectedPlate { get; set; }

    /// <summary>
    /// Tỷ lệ phần trăm khớp biển số giữa OCR và đăng ký (0.0% -> 100.0%).
    /// </summary>
    public double? PlateSimilarityPercent { get; set; }

    /// <summary>
    /// Cờ bỏ qua kiểm tra biển số xe đối với thẻ này.
    /// </summary>
    public bool SkipValidatePlate { get; set; }

    /// <summary>
    /// Họ và tên người dùng sở hữu thẻ (nếu có).
    /// </summary>
    public string? UserFullName { get; set; }

    /// <summary>
    /// Số phòng / căn hộ cư dân.
    /// </summary>
    public string? UserRoom { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ của cư dân.
    /// </summary>
    public string? UserPhone { get; set; }

    /// <summary>
    /// ID phiên gửi xe (tạo mới khi vào hoặc cập nhật khi ra).
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// ID của sự kiện quẹt thẻ được lưu trong AccessEvents.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Đường dẫn file ảnh snapshot đã lưu trữ.
    /// </summary>
    public string? SnapshotPath { get; set; }

    /// <summary>
    /// Thời điểm quẹt thẻ (UTC).
    /// </summary>
    public DateTime EventTime { get; set; }
}
