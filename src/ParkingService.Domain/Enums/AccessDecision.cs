namespace ParkingService.Domain.Enums;

/// <summary>
/// Quyết định cho phép hoặc từ chối mở barrier khi quẹt thẻ ra/vào.
/// </summary>
public enum AccessDecision
{
    /// <summary>
    /// Cho phép xe qua barrier (hợp lệ)
    /// </summary>
    Allowed = 1,

    /// <summary>
    /// Từ chối xe qua barrier (thẻ sai, biển số không khớp, người dùng bị khóa...)
    /// </summary>
    Denied = 2
}
