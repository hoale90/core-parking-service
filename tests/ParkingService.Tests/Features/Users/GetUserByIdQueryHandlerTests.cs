using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Users.Queries.GetUserById;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;
using ParkingService.Domain.Exceptions;
using Xunit;

namespace ParkingService.Tests.Features.Users;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IRepository<Card>> _cardRepositoryMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IRepository<User>>();
        _cardRepositoryMock = new Mock<IRepository<Card>>();
        _handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object, _cardRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var missingUserId = Guid.NewGuid();
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(missingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetUserByIdQuery(missingUserId);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.WithMessage($"*User*{missingUserId}*");
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnCorrectUserDtoWithLinkedCard()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FullName = "Đặng Hoàng Phúc",
            Room = "B22",
            Email = "phuc.dang@example.com",
            Phone = "0933112233",
            Gender = "Nam",
            PlateNumber = "29A-888.88",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var card = new Card
        {
            Id = Guid.NewGuid(),
            CardNumber = "2848255313",
            UserId = userId,
            Status = CardStatus.Active
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _cardRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FullName.Should().Be("Đặng Hoàng Phúc");
        result.Room.Should().Be("B22");
        result.Email.Should().Be("phuc.dang@example.com");
        result.PlateNumber.Should().Be("29A-888.88");
        result.LinkedCardNumber.Should().Be("2848255313");
    }
}
