using MediatR;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Cards.Commands.CreateCard;

/// <summary>
/// Yêu cầu tạo mới thẻ gửi xe (RFID/Mifare).
/// </summary>
/// <param name="CardNumber">Mã số định danh của thẻ (bắt buộc, duy nhất, tối đa 50 ký tự)</param>
/// <param name="UserId">ID của người dùng được gán thẻ (Guid nullable - để trống đối với thẻ chưa gán hoặc thẻ khách)</param>
/// <param name="Type">Loại thẻ (1 = Registered, 2 = Temporary, 3 = Unknown)</param>
/// <param name="VehiclePlate">Biển số xe gắn với thẻ</param>
/// <param name="SkipValidateVehiclePlate">Cờ bỏ qua xác thực biển số xe</param>
/// <param name="Info">Thông tin chủ thẻ / ghi chú</param>
/// <param name="Phone">Số điện thoại liên hệ</param>
/// <param name="Room">Số phòng / căn hộ</param>
/// <param name="Status">Trạng thái thẻ (1 = Active, 2 = Blocked, 3 = Lost)</param>
public record CreateCardCommand(
    string CardNumber,
    Guid? UserId = null,
    CardType Type = CardType.Registered,
    string? VehiclePlate = null,
    bool SkipValidateVehiclePlate = false,
    string? Info = null,
    string? Phone = null,
    string? Room = null,
    CardStatus Status = CardStatus.Active
) : IRequest<CardDto>;
