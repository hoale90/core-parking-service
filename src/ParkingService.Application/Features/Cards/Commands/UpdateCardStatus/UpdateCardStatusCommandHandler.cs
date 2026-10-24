using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Exceptions;

namespace ParkingService.Application.Features.Cards.Commands.UpdateCardStatus;

public class UpdateCardStatusCommandHandler : IRequestHandler<UpdateCardStatusCommand, CardDto>
{
    private readonly IRepository<Card> _cardRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateCardStatusCommandHandler(
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

    public async Task<CardDto> Handle(UpdateCardStatusCommand request, CancellationToken cancellationToken)
    {
        var card = await _cardRepository.GetByIdAsync(request.CardId, cancellationToken);
        if (card == null)
        {
            throw new NotFoundException("Card", request.CardId);
        }

        // Cập nhật trạng thái thẻ (Option C: Giữ nguyên cư dân liên kết nếu có)
        card.Status = request.Status;
        card.UpdatedAt = _dateTimeProvider.UtcNow;

        await _cardRepository.UpdateAsync(card, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Lấy thông tin User liên kết nếu có để trả về CardDto đầy đủ
        User? linkedUser = null;
        if (card.UserId.HasValue)
        {
            linkedUser = await _userRepository.GetByIdAsync(card.UserId.Value, cancellationToken);
        }

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
