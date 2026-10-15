using MediatR;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Cards.Commands.CreateCard;

/// <summary>
/// Command to create a new card.
/// UserId is nullable to support unassigned cards or visitor cards.
/// </summary>
public record CreateCardCommand(
    string CardNumber,
    Guid? UserId = null,
    CardStatus Status = CardStatus.Active
) : IRequest<CardDto>;
