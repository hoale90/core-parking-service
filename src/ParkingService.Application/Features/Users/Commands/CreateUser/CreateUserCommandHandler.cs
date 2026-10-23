using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Users.DTOs;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;
using ParkingService.Domain.Exceptions;

namespace ParkingService.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Card> _cardRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateUserCommandHandler(
        IRepository<User> userRepository,
        IRepository<Card> cardRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _cardRepository = cardRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Nếu có CardId, kiểm tra thẻ có tồn tại và đã gán cho ai chưa
        Card? cardToLink = null;
        if (request.CardId.HasValue)
        {
            cardToLink = await _cardRepository.GetByIdAsync(request.CardId.Value, cancellationToken);
            if (cardToLink == null)
            {
                throw new NotFoundException("Card", request.CardId.Value);
            }

            if (cardToLink.UserId.HasValue)
            {
                throw new ConflictException("Card", "UserId", "Thẻ này đã được gán cho một người dùng khác.");
            }
        }

        // 2. Tạo User
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim().ToLowerInvariant(),
            PlateNumber = request.PlateNumber?.Trim().ToUpperInvariant(),
            Room = request.Room?.Trim(),
            DateOfBirth = request.DateOfBirth.HasValue
                ? DateTime.SpecifyKind(request.DateOfBirth.Value, DateTimeKind.Utc)
                : null,
            Gender = request.Gender?.Trim(),
            Status = request.Status,
            CreatedAt = _dateTimeProvider.UtcNow,
            IsDeleted = false
        };

        await _userRepository.AddAsync(user, cancellationToken);

        // 3. Liên kết thẻ nếu có
        if (cardToLink != null)
        {
            cardToLink.UserId = user.Id;
            cardToLink.Type = CardType.Registered;
            cardToLink.UpdatedAt = _dateTimeProvider.UtcNow;
            await _cardRepository.UpdateAsync(cardToLink, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            LinkedCardId = cardToLink?.Id,
            LinkedCardNumber = cardToLink?.CardNumber,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
