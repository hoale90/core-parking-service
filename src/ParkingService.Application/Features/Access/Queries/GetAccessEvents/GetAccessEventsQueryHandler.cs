using System.Linq.Expressions;
using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Access.DTOs;
using ParkingService.Domain.Entities;

namespace ParkingService.Application.Features.Access.Queries.GetAccessEvents;

public class GetAccessEventsQueryHandler : IRequestHandler<GetAccessEventsQuery, PaginatedResult<AccessEventDto>>
{
    private readonly IRepository<AccessEvent> _eventRepository;
    private readonly IRepository<User> _userRepository;

    public GetAccessEventsQueryHandler(
        IRepository<AccessEvent> eventRepository,
        IRepository<User> userRepository)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
    }

    public async Task<PaginatedResult<AccessEventDto>> Handle(GetAccessEventsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 500 ? 10 : request.PageSize;

        Expression<Func<AccessEvent, bool>> predicate = e =>
            (string.IsNullOrWhiteSpace(request.CardNumber) || e.CardNumber.Contains(request.CardNumber)) &&
            (!request.Direction.HasValue || e.Direction == request.Direction.Value) &&
            (!request.Decision.HasValue || e.Decision == request.Decision.Value) &&
            (!request.FromDate.HasValue || e.EventTime >= request.FromDate.Value) &&
            (!request.ToDate.HasValue || e.EventTime <= request.ToDate.Value);

        var pagedEvents = await _eventRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            predicate,
            q => q.OrderByDescending(e => e.EventTime),
            cancellationToken);

        var userIds = pagedEvents.Items
            .Where(e => e.UserId.HasValue)
            .Select(e => e.UserId!.Value)
            .Distinct()
            .ToList();

        var usersMap = userIds.Count > 0
            ? (await _userRepository.FindAsync(u => userIds.Contains(u.Id), cancellationToken)).ToDictionary(u => u.Id)
            : new Dictionary<Guid, User>();

        return new PaginatedResult<AccessEventDto>
        {
            Items = pagedEvents.Items.Select(e =>
            {
                var user = e.UserId.HasValue && usersMap.TryGetValue(e.UserId.Value, out var u) ? u : null;
                return new AccessEventDto
                {
                    Id = e.Id,
                    CardNumber = e.CardNumber,
                    CardId = e.CardId,
                    UserId = e.UserId,
                    UserFullName = user?.FullName,
                    UserRoom = user?.Room,
                    Direction = e.Direction,
                    LaneCode = e.LaneCode,
                    OcrPlateNumber = e.OcrPlateNumber,
                    PlateNormalized = e.PlateNormalized,
                    ExpectedPlateNumber = e.ExpectedPlateNumber,
                    PlateSimilarityPercent = e.PlateSimilarityPercent,
                    SkipValidatePlate = e.SkipValidatePlate,
                    Decision = e.Decision,
                    ReasonCode = e.ReasonCode,
                    SnapshotPath = e.SnapshotPath,
                    EventTime = e.EventTime,
                    CreatedAt = e.CreatedAt
                };
            }).ToList(),
            TotalCount = pagedEvents.TotalCount,
            PageNumber = pagedEvents.PageNumber,
            PageSize = pagedEvents.PageSize
        };
    }
}
