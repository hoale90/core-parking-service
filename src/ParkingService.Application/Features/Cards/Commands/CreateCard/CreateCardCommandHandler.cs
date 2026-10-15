using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Exceptions;

namespace ParkingService.Application.Features.Cards.Commands.CreateCard;

public class CreateCardCommandHandler : IRequestHandler<CreateCardCommand, CardDto>
{
    private readonly IRepository<Card> _cardRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateCardCommandHandler(
        IRepository<Card> cardRepository,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _cardRepository = cardRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<CardDto> Handle(CreateCardCommand request, CancellationToken cancellationToken)
    {
        var normalizedCardNumber = request.CardNumber.Trim();

        // 1. Kiểm tra tính duy nhất của mã số thẻ
        var cardExists = await _cardRepository.AnyAsync(
            c => c.CardNumber.ToLower() == normalizedCardNumber.ToLower(),
            cancellationToken);

        if (cardExists)
        {
            throw new ConflictException("Card", "CardNumber", normalizedCardNumber);
        }

        // 2. Nếu UserId có giá trị, kiểm tra người dùng đó có tồn tại trong hệ thống không
        if (request.UserId.HasValue)
        {
            var userExists = await _userRepository.ExistsAsync(request.UserId.Value, cancellationToken);
            if (!userExists)
            {
                throw new NotFoundException("User", request.UserId.Value);
            }
        }

        // 3. Tạo mới thẻ
        var card = new Card
        {
            Id = Guid.NewGuid(),
            CardNumber = normalizedCardNumber,
            UserId = request.UserId,
            Status = request.Status,
            CreatedAt = _dateTimeProvider.UtcNow,
            IsDeleted = false
        };

        await _cardRepository.AddAsync(card, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CardDto
        {
            Id = card.Id,
            CardNumber = card.CardNumber,
            UserId = card.UserId,
            Status = card.Status,
            CreatedAt = card.CreatedAt,
            UpdatedAt = card.UpdatedAt
        };
    }
}
