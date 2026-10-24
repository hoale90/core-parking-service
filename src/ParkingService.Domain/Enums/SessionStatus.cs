namespace ParkingService.Domain.Enums;

/// <summary>
/// Trạng thái của một phiên gửi xe (từ lúc vào đến lúc ra).
/// </summary>
public enum SessionStatus
{
    /// <summary>
    /// Xe hiện đang đỗ bên trong bãi
    /// </summary>
    Inside = 1,

    /// <summary>
    /// Xe đã hoàn tất phiên gửi và rời khỏi bãi
    /// </summary>
    Exited = 2
}
