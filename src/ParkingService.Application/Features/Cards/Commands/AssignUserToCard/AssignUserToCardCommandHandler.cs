using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;
using ParkingService.Domain.Exceptions;

namespace ParkingService.Application.Features.Cards.Commands.AssignUserToCard;

public class AssignUserToCardCommandHandler : IRequestHandler<AssignUserToCardCommand, CardDto>
{
    private readonly IRepository<Card> _cardRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AssignUserToCardCommandHandler(
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

    public async Task<CardDto> Handle(AssignUserToCardCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra thẻ có tồn tại không
        var card = await _cardRepository.GetByIdAsync(request.CardId, cancellationToken);
        if (card == null)
        {
            throw new NotFoundException("Card", request.CardId);
        }

        User? linkedUser = null;

        // 2. Nếu có UserId: kiểm tra user hợp lệ & kiểm tra 1 user 1 thẻ
        if (request.UserId.HasValue)
        {
            linkedUser = await _userRepository.GetByIdAsync(request.UserId.Value, cancellationToken);
            if (linkedUser == null)
            {
                throw new NotFoundException("User", request.UserId.Value);
            }

            if (linkedUser.Status == UserStatus.Disabled)
            {
                throw new DomainException("Không thể liên kết thẻ với người dùng đang bị vô hiệu hóa.");
            }

            // Kiểm tra xem User này đã được liên kết với một thẻ khác chưa (trừ thẻ hiện tại)
            var userHasAnotherCard = await _cardRepository.AnyAsync(
                c => c.UserId == request.UserId.Value && c.Id != card.Id,
                cancellationToken);

            if (userHasAnotherCard)
            {
                throw new ConflictException("Card", "UserId", "Người dùng này đã được liên kết với một thẻ khác trong hệ thống.");
            }

            card.UserId = request.UserId.Value;
            card.Type = CardType.Registered;
        }
        else
        {
            // Hủy liên kết thẻ
            card.UserId = null;
        }

        card.UpdatedAt = _dateTimeProvider.UtcNow;
        await _cardRepository.UpdateAsync(card, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CardDto
        {
            Id = card.Id,
            CardNumber = card.CardNumber,
            Type = card.Type,
            UserId = card.UserId,
            Status = card.Status,
            UserFullName = linkedUser?.FullName,
            UserPlateNumber = linkedUser?.PlateNumber,
            UserPhone = linkedUser?.Phone,
            UserRoom = linkedUser?.Room,
            UserStatus = linkedUser?.Status,
            CreatedAt = card.CreatedAt,
            UpdatedAt = card.UpdatedAt
        };
    }
}
