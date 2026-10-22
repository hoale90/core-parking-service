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

        // Assert - Realistic ConflictException verification
        var exception = await act.Should().ThrowAsync<ConflictException>();
        exception.WithMessage($"*Card*{existingCardNumber}*");

        // Verify that AddAsync and SaveChangesAsync were NEVER called
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
            .Setup(r => r.ExistsAsync(nonExistentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateCardCommand(
            CardNumber: "0099887766",
            UserId: nonExistentUserId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert - Realistic NotFoundException verification
        var exception = await act.Should().ThrowAsync<NotFoundException>();
        exception.WithMessage($"*User*{nonExistentUserId}*");

        // Verify that AddAsync was never called
        _cardRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()), Times.Never);
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
        result.CreatedAt.Should().Be(fixedTime);

        savedCard.Should().NotBeNull();
        savedCard!.UserId.Should().BeNull();
        savedCard.IsDeleted.Should().BeFalse();

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsValidAndExists_ShouldCreateCardLinkedToUser()
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
            .Setup(r => r.ExistsAsync(validUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

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

        // Assert
        result.Should().NotBeNull();
        result.CardNumber.Should().Be(cardNumber);
        result.UserId.Should().Be(validUserId);
        result.CreatedAt.Should().Be(fixedTime);

        savedCard.Should().NotBeNull();
        savedCard!.UserId.Should().Be(validUserId);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDataSqlFieldsAreProvided_ShouldPersistAllFieldsAccurately()
    {
        // Arrange - Realistic boarding house / apartment card creation from data.sql
        const string cardNumber = "2848255313";
        const string plateNumber = "94l1-05580";
        var fixedTime = new DateTime(2026, 10, 22, 10, 0, 0, DateTimeKind.Utc);
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
            Type: CardType.Registered,
            VehiclePlate: plateNumber,
            SkipValidateVehiclePlate: false,
            Info: "DUONG THI PHUNG",
            Phone: "0912345678",
            Room: "B22",
            Status: CardStatus.Active);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CardNumber.Should().Be(cardNumber);
        result.Type.Should().Be(CardType.Registered);
        result.VehiclePlate.Should().Be("94L1-05580"); // normalized to uppercase
        result.SkipValidateVehiclePlate.Should().BeFalse();
        result.Info.Should().Be("DUONG THI PHUNG");
        result.Phone.Should().Be("0912345678");
        result.Room.Should().Be("B22");

        savedCard.Should().NotBeNull();
        savedCard!.VehiclePlate.Should().Be("94L1-05580");
        savedCard.Room.Should().Be("B22");
        savedCard.Info.Should().Be("DUONG THI PHUNG");
        savedCard.Phone.Should().Be("0912345678");

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
