using MediatR;
using ParkingService.Application.Features.Cards.DTOs;

namespace ParkingService.Application.Features.Cards.Commands.UpdateCardSkipValidatePlate;

/// <summary>
/// Yêu cầu cập nhật cờ bỏ qua kiểm tra biển số xe của thẻ.
/// </summary>
public record UpdateCardSkipValidatePlateCommand(
    Guid CardId,
    bool SkipValidatePlate
) : IRequest<CardDto>;
