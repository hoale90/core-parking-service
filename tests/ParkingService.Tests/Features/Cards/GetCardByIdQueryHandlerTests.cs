using FluentAssertions;
using Moq;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Cards.Queries.GetCardById;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;
using ParkingService.Domain.Exceptions;
using Xunit;

namespace ParkingService.Tests.Features.Cards;

public class GetCardByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Card>> _cardRepositoryMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly GetCardByIdQueryHandler _handler;

    public GetCardByIdQueryHandlerTests()
    {
        _cardRepositoryMock = new Mock<IRepository<Card>>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _handler = new GetCardByIdQueryHandler(_cardRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCardDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var missingCardId = Guid.NewGuid();
        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(missingCardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Card?)null);

        var query = new GetCardByIdQuery(missingCardId);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.WithMessage($"*Card*{missingCardId}*");
    }

    [Fact]
    public async Task Handle_WhenCardExistsWithLinkedUser_ShouldReturnCardDtoWithUserPlateNumber()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var card = new Card
        {
            Id = cardId,
            CardNumber = "0088776655",
            Type = CardType.Registered,
            UserId = userId,
            Status = CardStatus.Active,
            SkipValidatePlate = false,
            CreatedAt = DateTime.UtcNow
        };

        var user = new User
        {
            Id = userId,
            FullName = "Đinh Hữu Đức",
            PlateNumber = "38B1-28228",
            Phone = "0901234567",
            Room = "A16",
            Status = UserStatus.Active
        };

        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetCardByIdQuery(cardId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(cardId);
        result.CardNumber.Should().Be("0088776655");
        result.Type.Should().Be(CardType.Registered);
        result.Status.Should().Be(CardStatus.Active);
        result.SkipValidatePlate.Should().BeFalse();
        result.UserId.Should().Be(userId);
        result.UserFullName.Should().Be("Đinh Hữu Đức");
        result.UserPlateNumber.Should().Be("38B1-28228");
        result.UserRoom.Should().Be("A16");
    }

    [Fact]
    public async Task Handle_WhenCardHasSkipValidatePlateTrue_ShouldReturnCardDtoWithSkipValidatePlateTrue()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var card = new Card
        {
            Id = cardId,
            CardNumber = "0108394918",
            Type = CardType.Registered,
            UserId = null,
            Status = CardStatus.Active,
            SkipValidatePlate = true,
            CreatedAt = DateTime.UtcNow
        };

        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var query = new GetCardByIdQuery(cardId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(cardId);
        result.CardNumber.Should().Be("0108394918");
        result.SkipValidatePlate.Should().BeTrue();
    }
}
