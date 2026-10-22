using FluentAssertions;
using Moq;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Users.Commands.CreateUser;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;
using ParkingService.Domain.Exceptions;
using Xunit;

namespace ParkingService.Tests.Features.Users;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IRepository<Card>> _cardRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IRepository<User>>();
        _cardRepositoryMock = new Mock<IRepository<Card>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _handler = new CreateUserCommandHandler(
            _userRepositoryMock.Object,
            _cardRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateUserAndReturnDto()
    {
        // Arrange
        var fixedTime = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(fixedTime);

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync((User u, CancellationToken _) => u);

        var command = new CreateUserCommand(
            FullName: "Nguyễn Văn An",
            Room: "A16",
            DateOfBirth: new DateTime(1995, 8, 20, 0, 0, 0, DateTimeKind.Utc),
            Gender: "Nam",
            Phone: "0901234567",
            Email: "AN.NGUYEN@GMAIL.COM",
            PlateNumber: "59-x1 123.45",
            Status: UserStatus.Active);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Return DTO
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.FullName.Should().Be("Nguyễn Văn An");
        result.Room.Should().Be("A16");
        result.Email.Should().Be("an.nguyen@gmail.com"); // normalized to lowercase
        result.PlateNumber.Should().Be("59-X1 123.45"); // normalized to uppercase
        result.Phone.Should().Be("0901234567");
        result.Gender.Should().Be("Nam");
        result.Status.Should().Be(UserStatus.Active);
        result.CreatedAt.Should().Be(fixedTime);

        // Assert - Entity persisted via repository
        capturedUser.Should().NotBeNull();
        capturedUser!.FullName.Should().Be("Nguyễn Văn An");
        capturedUser.Room.Should().Be("A16");
        capturedUser.Email.Should().Be("an.nguyen@gmail.com");
        capturedUser.PlateNumber.Should().Be("59-X1 123.45");
        capturedUser.CreatedAt.Should().Be(fixedTime);
        capturedUser.IsDeleted.Should().BeFalse();

        // Assert - Unit of work saved
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCardIdProvided_ShouldLinkUserAndCardTogether()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var card = new Card { Id = cardId, CardNumber = "CARD-001", UserId = null };

        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        var command = new CreateUserCommand(
            FullName: "Trần Thị Thủy Chi",
            PlateNumber: "21MD8-00208",
            CardId: cardId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.LinkedCardNumber.Should().Be("CARD-001");
        result.PlateNumber.Should().Be("21MD8-00208");

        card.UserId.Should().Be(result.Id);
        card.Type.Should().Be(CardType.Registered);

        _cardRepositoryMock.Verify(r => r.UpdateAsync(card, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCardAlreadyHasAnotherUser_ShouldThrowConflictException()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var card = new Card { Id = cardId, CardNumber = "CARD-001", UserId = Guid.NewGuid() };

        _cardRepositoryMock
            .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var command = new CreateUserCommand(
            FullName: "Người Dùng Mới",
            CardId: cardId);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert - Rule 1 User 1 Card
        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.WithMessage("*gán cho một người dùng khác*");
    }
}
