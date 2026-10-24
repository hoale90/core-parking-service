namespace ParkingService.Domain.Enums;

/// <summary>
/// Hướng di chuyển qua cổng kiểm soát xe.
/// </summary>
public enum AccessDirection
{
    /// <summary>
    /// Xe vào bãi
    /// </summary>
    In = 1,

    /// <summary>
    /// Xe ra khỏi bãi
    /// </summary>
    Out = 2
}
