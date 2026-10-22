using System.Linq.Expressions;
using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Domain.Entities;

namespace ParkingService.Application.Features.Cards.Queries.GetCards;

public class GetCardsQueryHandler : IRequestHandler<GetCardsQuery, PaginatedResult<CardDto>>
{
    private readonly IRepository<Card> _cardRepository;

    public GetCardsQueryHandler(IRepository<Card> cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public async Task<PaginatedResult<CardDto>> Handle(GetCardsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        Expression<Func<Card, bool>> predicate = c =>
            (!request.UserId.HasValue || c.UserId == request.UserId.Value) &&
            (!request.Status.HasValue || c.Status == request.Status.Value) &&
            (!request.Type.HasValue || c.Type == request.Type.Value) &&
            (string.IsNullOrWhiteSpace(request.Room) || c.Room == request.Room) &&
            (string.IsNullOrWhiteSpace(request.VehiclePlate) || (c.VehiclePlate != null && c.VehiclePlate.Contains(request.VehiclePlate))) &&
            (string.IsNullOrWhiteSpace(request.SearchTerm) ||
             c.CardNumber.Contains(request.SearchTerm) ||
             (c.VehiclePlate != null && c.VehiclePlate.Contains(request.SearchTerm)) ||
             (c.Info != null && c.Info.Contains(request.SearchTerm)) ||
             (c.Phone != null && c.Phone.Contains(request.SearchTerm)) ||
             (c.Room != null && c.Room.Contains(request.SearchTerm)));

        var pagedCards = await _cardRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            predicate,
            q => q.OrderByDescending(c => c.CreatedAt),
            cancellationToken);

        return new PaginatedResult<CardDto>
        {
            Items = pagedCards.Items.Select(c => new CardDto
            {
                Id = c.Id,
                CardNumber = c.CardNumber,
                Type = c.Type,
                VehiclePlate = c.VehiclePlate,
                SkipValidateVehiclePlate = c.SkipValidateVehiclePlate,
                Info = c.Info,
                Phone = c.Phone,
                Room = c.Room,
                UserId = c.UserId,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList(),
            TotalCount = pagedCards.TotalCount,
            PageNumber = pagedCards.PageNumber,
            PageSize = pagedCards.PageSize
        };
    }
}
