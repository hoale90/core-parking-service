using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Users.Queries.GetUsers;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;
using Xunit;

namespace ParkingService.Tests.Features.Users;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IRepository<Card>> _cardRepositoryMock;
    private readonly GetUsersQueryHandler _handler;

    public GetUsersQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IRepository<User>>();
        _cardRepositoryMock = new Mock<IRepository<Card>>();
        _handler = new GetUsersQueryHandler(_userRepositoryMock.Object, _cardRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUsersHaveAssignedCards_ShouldMapLinkedCardIdAndNumber()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FullName = "Nguyen Van B",
            Room = "A102",
            Status = UserStatus.Active
        };

        var card = new Card
        {
            Id = cardId,
            CardNumber = "CARD-9999",
            UserId = userId,
            Status = CardStatus.Active
        };

        _cardRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Card> { card });

        _userRepositoryMock
            .Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<User, bool>>?>(),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<User>
            {
                Items = new List<User> { user },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            });

        var query = new GetUsersQuery(PageNumber: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        var userDto = result.Items[0];
        userDto.Id.Should().Be(userId);
        userDto.LinkedCardId.Should().Be(cardId);
        userDto.LinkedCardNumber.Should().Be("CARD-9999");
    }

    [Fact]
    public async Task Handle_WithCardStatusAssigned_WhenNoCardsAssigned_ShouldReturnEmptyImmediately()
    {
        // Arrange
        _cardRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Card>()); // No cards assigned

        var query = new GetUsersQuery(PageNumber: 1, PageSize: 10, CardStatus: "assigned");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        _userRepositoryMock.Verify(r => r.GetPagedAsync(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Expression<Func<User, bool>>?>(),
            It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
