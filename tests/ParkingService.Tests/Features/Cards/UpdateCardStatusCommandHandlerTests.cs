using FluentAssertions;
using Moq;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Cards.Commands.UpdateCardStatus;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;
using ParkingService.Domain.Exceptions;
using Xunit;

namespace ParkingService.Tests.Features.Cards;

public class UpdateCardStatusCommandHandlerTests
{
    private readonly Mock<IRepository<Card>> _cardRepositoryMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly UpdateCardStatusCommandHandler _handler;

    public UpdateCardStatusCommandHandlerTests()
    {
        _cardRepositoryMock = new Mock<IRepository<Card>>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc));

        _handler = new UpdateCardStatusCommandHandler(
            _cardRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCardNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var missingCardId = Guid.NewGuid();
        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(missingCardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Card?)null);

        var command = new UpdateCardStatusCommand(missingCardId, CardStatus.Inactive);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDeactivatingAssignedCard_ShouldPreserveUserAssociation_OptionC()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var card = new Card
        {
            Id = cardId,
            CardNumber = "CARD-123456",
            Status = CardStatus.Active,
            UserId = userId
        };

        var user = new User
        {
            Id = userId,
            FullName = "Nguyễn Văn A",
            PlateNumber = "29A-99999",
            Phone = "0987654321",
            Room = "P101"
        };

        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new UpdateCardStatusCommand(cardId, CardStatus.Inactive);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        card.Status.Should().Be(CardStatus.Inactive);
        card.UserId.Should().Be(userId, "Option C preserves resident assignment even when inactive");
        result.Status.Should().Be(CardStatus.Inactive);
        result.UserId.Should().Be(userId);
        result.UserFullName.Should().Be("Nguyễn Văn A");

        _cardRepositoryMock.Verify(r => r.UpdateAsync(card, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenActivatingCard_ShouldSetActiveStatus()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var card = new Card
        {
            Id = cardId,
            CardNumber = "CARD-654321",
            Status = CardStatus.Inactive,
            UserId = null
        };

        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var command = new UpdateCardStatusCommand(cardId, CardStatus.Active);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        card.Status.Should().Be(CardStatus.Active);
        result.Status.Should().Be(CardStatus.Active);
        _cardRepositoryMock.Verify(r => r.UpdateAsync(card, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
