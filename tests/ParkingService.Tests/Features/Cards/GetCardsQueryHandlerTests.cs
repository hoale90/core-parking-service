using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Cards.Queries.GetCards;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;
using Xunit;

namespace ParkingService.Tests.Features.Cards;

public class GetCardsQueryHandlerTests
{
    private readonly Mock<IRepository<Card>> _cardRepositoryMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly GetCardsQueryHandler _handler;

    public GetCardsQueryHandlerTests()
    {
        _cardRepositoryMock = new Mock<IRepository<Card>>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _handler = new GetCardsQueryHandler(_cardRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithPageSize1000_ShouldAllowLargePageSizeWithoutClampingTo10()
    {
        // Arrange
        var capturedPageSize = 0;
        _cardRepositoryMock
            .Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Card, bool>>?>(),
                It.IsAny<Func<IQueryable<Card>, IOrderedQueryable<Card>>?>(),
                It.IsAny<CancellationToken>()))
            .Callback<int, int, Expression<Func<Card, bool>>?, Func<IQueryable<Card>, IOrderedQueryable<Card>>?, CancellationToken>(
                (pageNumber, pageSize, pred, order, ct) => capturedPageSize = pageSize)
            .ReturnsAsync(new PaginatedResult<Card>
            {
                Items = new List<Card>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 1000
            });

        var query = new GetCardsQuery(PageNumber: 1, PageSize: 1000, Status: "all");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        capturedPageSize.Should().Be(1000);
        result.PageSize.Should().Be(1000);
    }

    [Fact]
    public async Task Handle_WithAllStatus_ShouldPassThrough()
    {
        // Arrange
        _cardRepositoryMock
            .Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Card, bool>>?>(),
                It.IsAny<Func<IQueryable<Card>, IOrderedQueryable<Card>>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<Card>
            {
                Items = new List<Card>
                {
                    new() { Id = Guid.NewGuid(), CardNumber = "CARD-001", Status = CardStatus.Active, UserId = null }
                },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            });

        var query = new GetCardsQuery(PageNumber: 1, PageSize: 10, Status: "all");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].CardNumber.Should().Be("CARD-001");
    }

    [Fact]
    public async Task Handle_WhenCardHasAssignedUser_ShouldPopulateUserInformation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FullName = "Nguyen Van A",
            Phone = "0901234567",
            PlateNumber = "51F-123.45",
            Room = "P101"
        };

        var card = new Card
        {
            Id = Guid.NewGuid(),
            CardNumber = "CARD-ASSIGNED",
            Status = CardStatus.Active,
            UserId = userId
        };

        _cardRepositoryMock
            .Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Card, bool>>?>(),
                It.IsAny<Func<IQueryable<Card>, IOrderedQueryable<Card>>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<Card>
            {
                Items = new List<Card> { card },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            });

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        var query = new GetCardsQuery(PageNumber: 1, PageSize: 10, Status: "assigned");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        var cardDto = result.Items[0];
        cardDto.UserId.Should().Be(userId);
        cardDto.UserFullName.Should().Be("Nguyen Van A");
        cardDto.UserPlateNumber.Should().Be("51F-123.45");
        cardDto.UserPhone.Should().Be("0901234567");
        cardDto.UserRoom.Should().Be("P101");
    }
}
