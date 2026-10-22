using MediatR;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Users.DTOs;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    UserStatus? Status = null,
    string? Room = null,
    string? SearchTerm = null
) : IRequest<PaginatedResult<UserDto>>;
