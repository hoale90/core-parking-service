using System.Linq.Expressions;
using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Cards.Queries.GetCards;

public class GetCardsQueryHandler : IRequestHandler<GetCardsQuery, PaginatedResult<CardDto>>
{
    private readonly IRepository<Card> _cardRepository;
    private readonly IRepository<User> _userRepository;

    public GetCardsQueryHandler(
        IRepository<Card> cardRepository,
        IRepository<User> userRepository)
    {
        _cardRepository = cardRepository;
        _userRepository = userRepository;
    }

    public async Task<PaginatedResult<CardDto>> Handle(GetCardsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 1000 ? 10 : request.PageSize;

        var statusStr = request.Status?.Trim().ToLowerInvariant();
        CardStatus? specificStatus = statusStr switch
        {
            "inactive" or "locked" or "blocked" or "lost" or "2" or "3" or "không hoạt động" => CardStatus.Inactive,
            "active" or "1" or "hoạt động" => CardStatus.Active,
            _ => null
        };
        bool filterAvailable = statusStr is "available" or "unassigned";
        bool filterAssigned = statusStr is "assigned";

        Expression<Func<Card, bool>> predicate = c =>
            !string.IsNullOrWhiteSpace(c.CardNumber) &&
            (!request.UserId.HasValue || c.UserId == request.UserId.Value) &&
            (!filterAvailable || (c.UserId == null && c.Status == CardStatus.Active)) &&
            (!filterAssigned || c.UserId != null) &&
            (!specificStatus.HasValue || c.Status == specificStatus.Value) &&
            (!request.Type.HasValue || c.Type == request.Type.Value) &&
            (string.IsNullOrWhiteSpace(request.SearchTerm) || c.CardNumber.Contains(request.SearchTerm));

        var pagedCards = await _cardRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            predicate,
            q => q.OrderByDescending(c => c.CreatedAt),
            cancellationToken);

        // Nạp trước danh sách Users liên kết để tránh N+1 queries
        var userIds = pagedCards.Items
            .Where(c => c.UserId.HasValue)
            .Select(c => c.UserId!.Value)
            .Distinct()
            .ToList();

        var usersMap = userIds.Count > 0
            ? (await _userRepository.FindAsync(u => userIds.Contains(u.Id), cancellationToken)).ToDictionary(u => u.Id)
            : new Dictionary<Guid, User>();

        return new PaginatedResult<CardDto>
        {
            Items = pagedCards.Items.Select(c =>
            {
                var user = c.UserId.HasValue && usersMap.TryGetValue(c.UserId.Value, out var u) ? u : null;
                return new CardDto
                {
                    Id = c.Id,
                    CardNumber = c.CardNumber,
                    Type = c.Type,
                    UserId = c.UserId,
                    Status = c.Status,
                    UserFullName = user?.FullName,
                    UserPlateNumber = user?.PlateNumber,
                    UserPhone = user?.Phone,
                    UserRoom = user?.Room,
                    UserStatus = user?.Status,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                };
            }).ToList(),
            TotalCount = pagedCards.TotalCount,
            PageNumber = pagedCards.PageNumber,
            PageSize = pagedCards.PageSize
        };
    }
}
