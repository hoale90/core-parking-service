using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Exceptions;

namespace ParkingService.Application.Features.Cards.Queries.GetCardById;

public class GetCardByIdQueryHandler : IRequestHandler<GetCardByIdQuery, CardDto>
{
    private readonly IRepository<Card> _cardRepository;

    public GetCardByIdQueryHandler(IRepository<Card> cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public async Task<CardDto> Handle(GetCardByIdQuery request, CancellationToken cancellationToken)
    {
        var card = await _cardRepository.GetByIdAsync(request.Id, cancellationToken);
        if (card == null)
        {
            throw new NotFoundException("Card", request.Id);
        }

        return new CardDto
        {
            Id = card.Id,
            CardNumber = card.CardNumber,
            Type = card.Type,
            VehiclePlate = card.VehiclePlate,
            SkipValidateVehiclePlate = card.SkipValidateVehiclePlate,
            Info = card.Info,
            Phone = card.Phone,
            Room = card.Room,
            UserId = card.UserId,
            Status = card.Status,
            CreatedAt = card.CreatedAt,
            UpdatedAt = card.UpdatedAt
        };
    }
}
