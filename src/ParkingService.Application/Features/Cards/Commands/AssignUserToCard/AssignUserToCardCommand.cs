using MediatR;
using ParkingService.Application.Features.Cards.DTOs;

namespace ParkingService.Application.Features.Cards.Commands.AssignUserToCard;

/// <summary>
/// Yêu cầu liên kết hoặc hủy liên kết người dùng với một thẻ gửi xe (1 User - 1 Thẻ).
/// </summary>
/// <param name="CardId">ID của thẻ</param>
/// <param name="UserId">ID của người dùng cần liên kết (truyền null nếu muốn hủy liên kết thẻ)</param>
public record AssignUserToCardCommand(
    Guid CardId,
    Guid? UserId = null
) : IRequest<CardDto>;
