using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Cards.Commands.AssignUserToCard;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;
using ParkingService.Domain.Exceptions;
using Xunit;

namespace ParkingService.Tests.Features.Cards;

public class AssignUserToCardCommandHandlerTests
{
    private readonly Mock<IRepository<Card>> _cardRepositoryMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly AssignUserToCardCommandHandler _handler;

    public AssignUserToCardCommandHandlerTests()
    {
        _cardRepositoryMock = new Mock<IRepository<Card>>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _handler = new AssignUserToCardCommandHandler(
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

        var command = new AssignUserToCardCommand(missingCardId, Guid.NewGuid());

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.WithMessage($"*Card*{missingCardId}*");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var missingUserId = Guid.NewGuid();
        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Card { Id = cardId, CardNumber = "CARD-001" });

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(missingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new AssignUserToCardCommand(cardId, missingUserId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.WithMessage($"*User*{missingUserId}*");
    }

    [Fact]
    public async Task Handle_WhenUserIsDisabled_ShouldThrowDomainException()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var disabledUserId = Guid.NewGuid();
        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Card { Id = cardId, CardNumber = "CARD-001" });

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(disabledUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = disabledUserId, FullName = "Bị khóa", Status = UserStatus.Disabled });

        var command = new AssignUserToCardCommand(cardId, disabledUserId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.WithMessage("*vô hiệu hóa*");
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyHasAnotherCard_ShouldThrowConflictException()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Card { Id = cardId, CardNumber = "CARD-001" });

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, FullName = "Nguyễn Văn B", Status = UserStatus.Active });

        _cardRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new AssignUserToCardCommand(cardId, userId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.WithMessage("*liên kết với một thẻ khác*");
    }

    [Fact]
    public async Task Handle_WhenValidUser_ShouldSuccessfullyAssignUserToCard()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var card = new Card { Id = cardId, CardNumber = "CARD-001", Status = CardStatus.Active };
        var user = new User
        {
            Id = userId,
            FullName = "Đinh Hữu Đức",
            PlateNumber = "38B1-28228",
            Room = "A16",
            Status = UserStatus.Active
        };

        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _cardRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new AssignUserToCardCommand(cardId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Successfully assigned
        result.Should().NotBeNull();
        result.Id.Should().Be(cardId);
        result.UserId.Should().Be(userId);
        result.UserFullName.Should().Be("Đinh Hữu Đức");
        result.UserPlateNumber.Should().Be("38B1-28228");
        result.UserRoom.Should().Be("A16");
        result.Type.Should().Be(CardType.Registered);

        card.UserId.Should().Be(userId);
        _cardRepositoryMock.Verify(r => r.UpdateAsync(card, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldUnassignCard()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var card = new Card { Id = cardId, CardNumber = "CARD-001", UserId = Guid.NewGuid() };

        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var command = new AssignUserToCardCommand(cardId, UserId: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.UserId.Should().BeNull();
        result.UserFullName.Should().BeNull();
        card.UserId.Should().BeNull();

        _cardRepositoryMock.Verify(r => r.UpdateAsync(card, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
