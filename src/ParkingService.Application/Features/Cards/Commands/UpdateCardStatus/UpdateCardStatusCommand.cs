using MediatR;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Cards.Commands.UpdateCardStatus;

/// <summary>
/// Yêu cầu cập nhật trạng thái hoạt động của thẻ (Active/Inactive).
/// </summary>
public record UpdateCardStatusCommand(
    Guid CardId,
    CardStatus Status
) : IRequest<CardDto>;
