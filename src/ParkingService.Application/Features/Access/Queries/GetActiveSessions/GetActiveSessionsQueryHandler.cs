using System.Linq.Expressions;
using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Access.DTOs;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Access.Queries.GetActiveSessions;

public class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, PaginatedResult<ParkingSessionDto>>
{
    private readonly IRepository<ParkingSession> _sessionRepository;
    private readonly IRepository<User> _userRepository;

    public GetActiveSessionsQueryHandler(
        IRepository<ParkingSession> sessionRepository,
        IRepository<User> userRepository)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
    }

    public async Task<PaginatedResult<ParkingSessionDto>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 500 ? 10 : request.PageSize;

        Expression<Func<ParkingSession, bool>> predicate = s =>
            s.Status == SessionStatus.Inside &&
            (string.IsNullOrWhiteSpace(request.SearchTerm) ||
             s.CardNumber.Contains(request.SearchTerm) ||
             (s.PlateNumber != null && s.PlateNumber.Contains(request.SearchTerm)));

        var pagedSessions = await _sessionRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            predicate,
            q => q.OrderByDescending(s => s.EntryTime),
            cancellationToken);

        var userIds = pagedSessions.Items
            .Where(s => s.UserId.HasValue)
            .Select(s => s.UserId!.Value)
            .Distinct()
            .ToList();

        var usersMap = userIds.Count > 0
            ? (await _userRepository.FindAsync(u => userIds.Contains(u.Id), cancellationToken)).ToDictionary(u => u.Id)
            : new Dictionary<Guid, User>();

        return new PaginatedResult<ParkingSessionDto>
        {
            Items = pagedSessions.Items.Select(s =>
            {
                var user = s.UserId.HasValue && usersMap.TryGetValue(s.UserId.Value, out var u) ? u : null;
                return new ParkingSessionDto
                {
                    Id = s.Id,
                    CardNumber = s.CardNumber,
                    CardId = s.CardId,
                    UserId = s.UserId,
                    UserFullName = user?.FullName,
                    UserRoom = user?.Room,
                    PlateNumber = s.PlateNumber,
                    EntryTime = s.EntryTime,
                    ExitTime = s.ExitTime,
                    DurationMinutes = s.DurationMinutes,
                    EntrySnapshotPath = s.EntrySnapshotPath,
                    ExitSnapshotPath = s.ExitSnapshotPath,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt
                };
            }).ToList(),
            TotalCount = pagedSessions.TotalCount,
            PageNumber = pagedSessions.PageNumber,
            PageSize = pagedSessions.PageSize
        };
    }
}
