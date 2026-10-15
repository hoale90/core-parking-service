using MediatR;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Cards.Commands.CreateCard;

/// <summary>
/// Yêu cầu tạo mới thẻ gửi xe (RFID/Mifare).
/// </summary>
/// <param name="CardNumber">Mã số định danh của thẻ (bắt buộc, duy nhất, tối đa 50 ký tự)</param>
/// <param name="UserId">ID của người dùng được gán thẻ (Guid nullable - để trống đối với thẻ chưa gán hoặc thẻ khách)</param>
/// <param name="Status">Trạng thái thẻ (1 = Active, 2 = Blocked, 3 = Lost)</param>
public record CreateCardCommand(
    string CardNumber,
    Guid? UserId = null,
    CardStatus Status = CardStatus.Active
) : IRequest<CardDto>;
