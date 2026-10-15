using MediatR;
using ParkingService.Application.Features.Users.DTOs;

namespace ParkingService.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;
