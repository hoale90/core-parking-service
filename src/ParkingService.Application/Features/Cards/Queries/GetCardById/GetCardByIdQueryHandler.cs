using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Exceptions;

namespace ParkingService.Application.Features.Cards.Queries.GetCardById;

public class GetCardByIdQueryHandler : IRequestHandler<GetCardByIdQuery, CardDto>
{
    private readonly IRepository<Card> _cardRepository;
    private readonly IRepository<User> _userRepository;

    public GetCardByIdQueryHandler(
        IRepository<Card> cardRepository,
        IRepository<User> userRepository)
    {
        _cardRepository = cardRepository;
        _userRepository = userRepository;
    }

    public async Task<CardDto> Handle(GetCardByIdQuery request, CancellationToken cancellationToken)
    {
        var card = await _cardRepository.GetByIdAsync(request.Id, cancellationToken);
        if (card == null)
        {
            throw new NotFoundException("Card", request.Id);
        }

        User? user = null;
        if (card.UserId.HasValue)
        {
            user = await _userRepository.GetByIdAsync(card.UserId.Value, cancellationToken);
        }

        return new CardDto
        {
            Id = card.Id,
            CardNumber = card.CardNumber,
            Type = card.Type,
            UserId = card.UserId,
            Status = card.Status,
            SkipValidatePlate = card.SkipValidatePlate,
            UserFullName = user?.FullName,
            UserPlateNumber = user?.PlateNumber,
            UserPhone = user?.Phone,
            UserRoom = user?.Room,
            UserStatus = user?.Status,
            CreatedAt = card.CreatedAt,
            UpdatedAt = card.UpdatedAt
        };
    }
}
