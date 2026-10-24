using MediatR;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Access.DTOs;

namespace ParkingService.Application.Features.Access.Queries.GetActiveSessions;

/// <summary>
/// Truy vấn danh sách xe đang gửi bên trong bãi (Status = Inside).
/// </summary>
public record GetActiveSessionsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null
) : IRequest<PaginatedResult<ParkingSessionDto>>;
