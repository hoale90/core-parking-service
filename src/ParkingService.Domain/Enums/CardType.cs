namespace ParkingService.Domain.Enums;

/// <summary>
/// Loại thẻ gửi xe:
/// - Registered: Thẻ đăng ký (thẻ tháng / cư dân)
/// - Temporary: Thẻ tạm / vãng lai (khách gửi theo lượt)
/// - Unknown: Thẻ chưa xác định / thẻ mới quẹt lần đầu chưa đăng ký
/// </summary>
public enum CardType
{
    Registered = 1,
    Temporary = 2,
    Unknown = 3
}
