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
    private readonly GetCardByIdQueryHandler _handler;

    public GetCardByIdQueryHandlerTests()
    {
        _cardRepositoryMock = new Mock<IRepository<Card>>();
        _handler = new GetCardByIdQueryHandler(_cardRepositoryMock.Object);
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
    public async Task Handle_WhenCardExists_ShouldReturnCorrectCardDto()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var card = new Card
        {
            Id = cardId,
            CardNumber = "0088776655",
            Type = CardType.Registered,
            VehiclePlate = "59-X1 123.45",
            SkipValidateVehiclePlate = false,
            Info = "Nguyen Van A",
            Phone = "0901234567",
            Room = "P101",
            UserId = userId,
            Status = CardStatus.Active,
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
        result.CardNumber.Should().Be("0088776655");
        result.Type.Should().Be(CardType.Registered);
        result.VehiclePlate.Should().Be("59-X1 123.45");
        result.SkipValidateVehiclePlate.Should().BeFalse();
        result.Info.Should().Be("Nguyen Van A");
        result.Phone.Should().Be("0901234567");
        result.Room.Should().Be("P101");
        result.UserId.Should().Be(userId);
    }
}
