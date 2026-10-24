using FluentAssertions;
using Moq;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Cards.Commands.UpdateCardSkipValidatePlate;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;
using ParkingService.Domain.Exceptions;
using Xunit;

namespace ParkingService.Tests.Features.Cards;

public class UpdateCardSkipValidatePlateCommandHandlerTests
{
    private readonly Mock<IRepository<Card>> _cardRepositoryMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly UpdateCardSkipValidatePlateCommandHandler _handler;

    public UpdateCardSkipValidatePlateCommandHandlerTests()
    {
        _cardRepositoryMock = new Mock<IRepository<Card>>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _handler = new UpdateCardSkipValidatePlateCommandHandler(
            _cardRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCardDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Card?)null);

        var command = new UpdateCardSkipValidatePlateCommand(cardId, true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _cardRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCardExists_ShouldUpdateSkipValidatePlateToTrueAndTouchUpdatedAt()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var fixedTime = new DateTime(2026, 10, 24, 10, 30, 0, DateTimeKind.Utc);
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(fixedTime);

        var card = new Card
        {
            Id = cardId,
            CardNumber = "0108394918",
            SkipValidatePlate = false,
            Status = CardStatus.Active
        };

        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var command = new UpdateCardSkipValidatePlateCommand(cardId, true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(cardId);
        result.SkipValidatePlate.Should().BeTrue();
        result.UpdatedAt.Should().Be(fixedTime);

        card.SkipValidatePlate.Should().BeTrue();
        card.UpdatedAt.Should().Be(fixedTime);

        _cardRepositoryMock.Verify(r => r.UpdateAsync(card, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCardHasLinkedUser_ShouldReturnFullUserAndPlateInfoWithUpdatedFlag()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fixedTime = new DateTime(2026, 10, 24, 10, 30, 0, DateTimeKind.Utc);
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(fixedTime);

        var card = new Card
        {
            Id = cardId,
            CardNumber = "0354547274",
            UserId = userId,
            SkipValidatePlate = true,
            Status = CardStatus.Active
        };

        var user = new User
        {
            Id = userId,
            FullName = "Nguyễn Văn VIP",
            PlateNumber = "51H-999.99",
            Room = "P1001",
            Status = UserStatus.Active
        };

        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Cập nhật lại thành false (không bỏ qua nữa)
        var command = new UpdateCardSkipValidatePlateCommand(cardId, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SkipValidatePlate.Should().BeFalse();
        result.UserId.Should().Be(userId);
        result.UserFullName.Should().Be("Nguyễn Văn VIP");
        result.UserPlateNumber.Should().Be("51H-999.99");
        result.UserRoom.Should().Be("P1001");

        card.SkipValidatePlate.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
