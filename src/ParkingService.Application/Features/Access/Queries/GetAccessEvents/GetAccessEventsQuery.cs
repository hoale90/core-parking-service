using MediatR;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Access.DTOs;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Access.Queries.GetAccessEvents;

/// <summary>
/// Truy vấn danh sách lịch sử quẹt thẻ và ra vào bãi xe (phân trang và bộ lọc).
/// </summary>
public record GetAccessEventsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? CardNumber = null,
    AccessDirection? Direction = null,
    AccessDecision? Decision = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<PaginatedResult<AccessEventDto>>;
