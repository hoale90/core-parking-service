using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Users.DTOs;
using ParkingService.Domain.Entities;

namespace ParkingService.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateUserCommandHandler(
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Username = request.Username?.Trim(),
            RoleId = request.RoleId,
            Role = request.Role?.Trim(),
            Room = request.Room?.Trim(),
            DateOfBirth = request.DateOfBirth.HasValue
                ? DateTime.SpecifyKind(request.DateOfBirth.Value, DateTimeKind.Utc)
                : null,
            Gender = request.Gender?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim().ToLowerInvariant(),
            PlateNumber = request.PlateNumber?.Trim().ToUpperInvariant(),
            Status = request.Status,
            CreatedAt = _dateTimeProvider.UtcNow,
            IsDeleted = false
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            RoleId = user.RoleId,
            Role = user.Role,
            Room = user.Room,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Phone = user.Phone,
            Email = user.Email,
            PlateNumber = user.PlateNumber,
            Status = user.Status,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
