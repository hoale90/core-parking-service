using ParkingService.Domain.Common;
using ParkingService.Domain.Enums;

namespace ParkingService.Domain.Entities;

/// <summary>
/// Thực thể theo dõi trọn vẹn một phiên gửi xe (từ lúc xe vào bãi cho tới khi xe rời khỏi bãi).
/// </summary>
public class ParkingSession : BaseEntity
{
    /// <summary>
    /// Mã số thẻ xe sử dụng trong phiên gửi xe.
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Khóa ngoại liên kết tới thẻ xe.
    /// </summary>
    public Guid? CardId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới người dùng sở hữu thẻ.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Biển số xe thực tế ghi nhận trong phiên gửi.
    /// </summary>
    public string? PlateNumber { get; set; }

    /// <summary>
    /// Thời điểm xe vào bãi (UTC).
    /// </summary>
    public DateTime EntryTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời điểm xe ra khỏi bãi (UTC, null nếu xe vẫn đang đỗ bên trong).
    /// </summary>
    public DateTime? ExitTime { get; set; }

    /// <summary>
    /// Tổng thời gian xe đỗ tính theo phút.
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Khóa ngoại trỏ tới sự kiện quẹt thẻ lúc xe vào.
    /// </summary>
    public Guid? EntryEventId { get; set; }

    /// <summary>
    /// Khóa ngoại trỏ tới sự kiện quẹt thẻ lúc xe ra.
    /// </summary>
    public Guid? ExitEventId { get; set; }

    /// <summary>
    /// Đường dẫn file ảnh chụp xe lúc vào.
    /// </summary>
    public string? EntrySnapshotPath { get; set; }

    /// <summary>
    /// Đường dẫn file ảnh chụp xe lúc ra.
    /// </summary>
    public string? ExitSnapshotPath { get; set; }

    /// <summary>
    /// Trạng thái phiên gửi xe (Inside = đang ở trong bãi, Exited = đã ra khỏi bãi).
    /// </summary>
    public SessionStatus Status { get; set; } = SessionStatus.Inside;

    // Navigation properties
    public Card? Card { get; set; }
    public User? User { get; set; }
    public AccessEvent? EntryEvent { get; set; }
    public AccessEvent? ExitEvent { get; set; }
}
