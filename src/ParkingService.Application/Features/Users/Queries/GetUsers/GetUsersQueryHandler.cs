using System.Linq.Expressions;
using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Users.DTOs;
using ParkingService.Domain.Entities;

namespace ParkingService.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedResult<UserDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Card> _cardRepository;

    public GetUsersQueryHandler(
        IRepository<User> userRepository,
        IRepository<Card> cardRepository)
    {
        _userRepository = userRepository;
        _cardRepository = cardRepository;
    }

    public async Task<PaginatedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 1000 ? 10 : request.PageSize;

        // Lấy danh sách thẻ đã gán để map và phục vụ filter theo trạng thái cấp thẻ
        var assignedCards = await _cardRepository.FindAsync(c => c.UserId != null, cancellationToken);
        var cardByUser = assignedCards
            .GroupBy(c => c.UserId!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        var assignedUserIds = cardByUser.Keys.ToHashSet();

        var cardStatusFilter = request.CardStatus?.Trim().ToLowerInvariant();
        bool filterUnassigned = cardStatusFilter is "unassigned" or "chua_co_the";
        bool filterAssigned = cardStatusFilter is "assigned" or "da_co_the" or "has-card";

        // Nếu lọc người đã có thẻ mà hiện tại chưa có thẻ nào được gán -> trả về rỗng ngay
        if (filterAssigned && assignedUserIds.Count == 0)
        {
            return new PaginatedResult<UserDto>
            {
                Items = new List<UserDto>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        bool filterUnassignedWithIds = filterUnassigned && assignedUserIds.Count > 0;

        Expression<Func<User, bool>> predicate = u =>
            (!request.Status.HasValue || u.Status == request.Status.Value) &&
            (string.IsNullOrWhiteSpace(request.Room) || u.Room == request.Room) &&
            (!filterAssigned || assignedUserIds.Contains(u.Id)) &&
            (!filterUnassignedWithIds || !assignedUserIds.Contains(u.Id)) &&
            (string.IsNullOrWhiteSpace(request.SearchTerm) ||
             u.FullName.Contains(request.SearchTerm) ||
             (u.Email != null && u.Email.Contains(request.SearchTerm)) ||
             (u.Phone != null && u.Phone.Contains(request.SearchTerm)) ||
             (u.PlateNumber != null && u.PlateNumber.Contains(request.SearchTerm)) ||
             (u.Room != null && u.Room.Contains(request.SearchTerm)));

        var pagedUsers = await _userRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            predicate,
            q => q.OrderByDescending(u => u.CreatedAt),
            cancellationToken);

        return new PaginatedResult<UserDto>
        {
            Items = pagedUsers.Items.Select(u =>
            {
                var card = cardByUser.TryGetValue(u.Id, out var c) ? c : null;
                return new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Email = u.Email,
                    PlateNumber = u.PlateNumber,
                    Room = u.Room,
                    DateOfBirth = u.DateOfBirth,
                    Gender = u.Gender,
                    Status = u.Status,
                    LinkedCardId = card?.Id,
                    LinkedCardNumber = card?.CardNumber,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                };
            }).ToList(),
            TotalCount = pagedUsers.TotalCount,
            PageNumber = pagedUsers.PageNumber,
            PageSize = pagedUsers.PageSize
        };
    }
}
