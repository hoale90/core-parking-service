using MediatR;
using ParkingService.Application.Features.Users.DTOs;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Command to create a new user.
/// </summary>
public record CreateUserCommand(
    string FullName,
    DateTime? DateOfBirth = null,
    string? Gender = null,
    string? Phone = null,
    string? Email = null,
    UserStatus Status = UserStatus.Active
) : IRequest<UserDto>;
