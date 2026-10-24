using MediatR;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Cards.Commands.CreateCard;

/// <summary>
/// Yêu cầu tạo mới thẻ gửi xe (RFID/Mifare).
/// </summary>
/// <param name="CardNumber">Mã số định danh của thẻ (bắt buộc, duy nhất, tối đa 50 ký tự)</param>
/// <param name="UserId">ID của người dùng được gán thẻ (Guid nullable - để trống đối với thẻ chưa gán hoặc thẻ khách)</param>
/// <param name="Type">Phân loại thẻ (1 = Registered, 2 = Temporary, 3 = Unknown)</param>
/// <param name="Status">Trạng thái thẻ (1 = Active, 2 = Inactive)</param>
/// <param name="SkipValidatePlate">Cờ bỏ qua kiểm tra khớp biển số xe khi quẹt thẻ ra/vào (mặc định false)</param>
public record CreateCardCommand(
    string CardNumber,
    Guid? UserId = null,
    CardType Type = CardType.Registered,
    CardStatus Status = CardStatus.Active,
    bool SkipValidatePlate = false
) : IRequest<CardDto>;
