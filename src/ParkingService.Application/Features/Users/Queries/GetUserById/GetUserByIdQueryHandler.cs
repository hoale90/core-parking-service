using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Users.DTOs;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Exceptions;

namespace ParkingService.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Card> _cardRepository;

    public GetUserByIdQueryHandler(
        IRepository<User> userRepository,
        IRepository<Card> cardRepository)
    {
        _userRepository = userRepository;
        _cardRepository = cardRepository;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User", request.Id);
        }

        var linkedCard = await _cardRepository.FirstOrDefaultAsync(c => c.UserId == user.Id, cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Phone = user.Phone,
            Email = user.Email,
            PlateNumber = user.PlateNumber,
            Room = user.Room,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Status = user.Status,
            LinkedCardNumber = linkedCard?.CardNumber,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
