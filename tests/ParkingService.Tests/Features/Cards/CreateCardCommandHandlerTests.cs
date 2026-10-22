using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Cards.Commands.CreateCard;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;
using ParkingService.Domain.Exceptions;
using Xunit;

namespace ParkingService.Tests.Features.Cards;

public class CreateCardCommandHandlerTests
{
    private readonly Mock<IRepository<Card>> _cardRepositoryMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly CreateCardCommandHandler _handler;

    public CreateCardCommandHandlerTests()
    {
        _cardRepositoryMock = new Mock<IRepository<Card>>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _handler = new CreateCardCommandHandler(
            _cardRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCardNumberAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        const string existingCardNumber = "0012345678";
        _cardRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateCardCommand(CardNumber: existingCardNumber);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ConflictException>();
        exception.WithMessage($"*Card*{existingCardNumber}*");

        _cardRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAssignedUserIdDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        _cardRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(nonExistentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new CreateCardCommand(
            CardNumber: "0099887766",
            UserId: nonExistentUserId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<NotFoundException>();
        exception.WithMessage($"*User*{nonExistentUserId}*");

        _cardRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyHasACard_ShouldThrowConflictExceptionForOneToOneRule()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _cardRepositoryMock
            .Setup(r => r.AnyAsync(It.Is<Expression<Func<Card, bool>>>(e => e.ToString().Contains("CardNumber")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, FullName = "Nguyễn Văn A" });

        // User already has a card
        _cardRepositoryMock
            .Setup(r => r.AnyAsync(It.Is<Expression<Func<Card, bool>>>(e => e.ToString().Contains("UserId")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateCardCommand(CardNumber: "CARD-NEW", UserId: userId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert - Rule 1 User 1 Card
        var exception = await act.Should().ThrowAsync<ConflictException>();
        exception.WithMessage("*gán một thẻ khác*");
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldSuccessfullyCreateUnassignedCard()
    {
        // Arrange
        const string cardNumber = "0055443322";
        var fixedTime = new DateTime(2026, 10, 15, 14, 0, 0, DateTimeKind.Utc);
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(fixedTime);

        _cardRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Card? savedCard = null;
        _cardRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()))
            .Callback<Card, CancellationToken>((c, _) => savedCard = c)
            .ReturnsAsync((Card c, CancellationToken _) => c);

        var command = new CreateCardCommand(
            CardNumber: cardNumber,
            UserId: null,
            Status: CardStatus.Active);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.CardNumber.Should().Be(cardNumber);
        result.UserId.Should().BeNull();
        result.Status.Should().Be(CardStatus.Active);
        result.Type.Should().Be(CardType.Registered);
        result.UserFullName.Should().BeNull();
        result.UserPlateNumber.Should().BeNull();
        result.CreatedAt.Should().Be(fixedTime);

        savedCard.Should().NotBeNull();
        savedCard!.UserId.Should().BeNull();
        savedCard.IsDeleted.Should().BeFalse();

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsValidAndExists_ShouldCreateCardLinkedToUserWithPlateNumber()
    {
        // Arrange
        var validUserId = Guid.NewGuid();
        const string cardNumber = "0077889900";
        var fixedTime = new DateTime(2026, 10, 15, 14, 15, 0, DateTimeKind.Utc);
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(fixedTime);

        _cardRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(validUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = validUserId,
                FullName = "Nguyễn Văn Nam",
                PlateNumber = "59-X1 123.45",
                Phone = "0901234567",
                Room = "A16",
                Status = UserStatus.Active
            });

        Card? savedCard = null;
        _cardRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()))
            .Callback<Card, CancellationToken>((c, _) => savedCard = c)
            .ReturnsAsync((Card c, CancellationToken _) => c);

        var command = new CreateCardCommand(
            CardNumber: cardNumber,
            UserId: validUserId,
            Status: CardStatus.Active);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Card returns linked user info including PlateNumber for validation
        result.Should().NotBeNull();
        result.CardNumber.Should().Be(cardNumber);
        result.UserId.Should().Be(validUserId);
        result.UserFullName.Should().Be("Nguyễn Văn Nam");
        result.UserPlateNumber.Should().Be("59-X1 123.45");
        result.UserRoom.Should().Be("A16");
        result.UserStatus.Should().Be(UserStatus.Active);
        result.CreatedAt.Should().Be(fixedTime);

        savedCard.Should().NotBeNull();
        savedCard!.UserId.Should().Be(validUserId);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
