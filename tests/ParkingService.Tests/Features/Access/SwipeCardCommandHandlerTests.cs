using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Common.Services;
using ParkingService.Application.Features.Access.Commands.SwipeCard;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;
using Xunit;

namespace ParkingService.Tests.Features.Access;

public class SwipeCardCommandHandlerTests
{
    private readonly Mock<IRepository<Card>> _cardRepositoryMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IRepository<AccessEvent>> _accessEventRepositoryMock;
    private readonly Mock<IRepository<ParkingSession>> _sessionRepositoryMock;
    private readonly IPlateComparisonService _plateComparisonService;
    private readonly Mock<ISnapshotStorageService> _snapshotStorageMock;
    private readonly Mock<IOcrService> _ocrServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly SwipeCardCommandHandler _handler;

    private readonly DateTime _fixedNow = new(2026, 10, 24, 8, 30, 0, DateTimeKind.Utc);

    public SwipeCardCommandHandlerTests()
    {
        _cardRepositoryMock = new Mock<IRepository<Card>>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _accessEventRepositoryMock = new Mock<IRepository<AccessEvent>>();
        _sessionRepositoryMock = new Mock<IRepository<ParkingSession>>();
        _plateComparisonService = new PlateComparisonService();
        _snapshotStorageMock = new Mock<ISnapshotStorageService>();
        _ocrServiceMock = new Mock<IOcrService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(_fixedNow);

        _handler = new SwipeCardCommandHandler(
            _cardRepositoryMock.Object,
            _userRepositoryMock.Object,
            _accessEventRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            _plateComparisonService,
            _snapshotStorageMock.Object,
            _ocrServiceMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCardDoesNotExist_ShouldReturnDeniedWithCardNotFoundReason()
    {
        // Arrange
        const string cardNumber = "UNKNOWN-CARD-999";
        _cardRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Card?)null);

        AccessEvent? savedEvent = null;
        _accessEventRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<AccessEvent>(), It.IsAny<CancellationToken>()))
            .Callback<AccessEvent, CancellationToken>((e, _) => savedEvent = e)
            .ReturnsAsync((AccessEvent e, CancellationToken _) => e);

        var command = new SwipeCardCommand(CardNumber: cardNumber, Direction: AccessDirection.In);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Decision.Should().Be(AccessDecision.Denied);
        result.ReasonCode.Should().Be("CARD_NOT_FOUND");
        result.CardNumber.Should().Be(cardNumber);

        savedEvent.Should().NotBeNull();
        savedEvent!.Decision.Should().Be(AccessDecision.Denied);
        savedEvent.ReasonCode.Should().Be("CARD_NOT_FOUND");

        _sessionRepositoryMock.Verify(s => s.AddAsync(It.IsAny<ParkingSession>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCardIsInactive_ShouldReturnDeniedWithCardInactiveReason()
    {
        // Arrange
        const string cardNumber = "CARD-LOCKED-01";
        var card = new Card
        {
            Id = Guid.NewGuid(),
            CardNumber = cardNumber,
            Status = CardStatus.Inactive
        };

        _cardRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var command = new SwipeCardCommand(CardNumber: cardNumber, Direction: AccessDirection.In);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Decision.Should().Be(AccessDecision.Denied);
        result.ReasonCode.Should().Be("CARD_INACTIVE");
        _sessionRepositoryMock.Verify(s => s.AddAsync(It.IsAny<ParkingSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsDisabled_ShouldReturnDeniedWithUserDisabledReason()
    {
        // Arrange
        const string cardNumber = "0012345678";
        var userId = Guid.NewGuid();
        var card = new Card
        {
            Id = Guid.NewGuid(),
            CardNumber = cardNumber,
            UserId = userId,
            Status = CardStatus.Active
        };

        var user = new User
        {
            Id = userId,
            FullName = "Trần Văn Khóa",
            PlateNumber = "59-X1 123.45",
            Status = UserStatus.Disabled
        };

        _cardRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new SwipeCardCommand(CardNumber: cardNumber, Direction: AccessDirection.In);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Decision.Should().Be(AccessDecision.Denied);
        result.ReasonCode.Should().Be("USER_DISABLED");
        _sessionRepositoryMock.Verify(s => s.AddAsync(It.IsAny<ParkingSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCardAndPlateMatch_ShouldReturnAllowedAndCreateParkingSessionOnEntry()
    {
        // Arrange
        const string cardNumber = "0012345678";
        const string plateNumber = "59-X1 123.45";
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var card = new Card
        {
            Id = cardId,
            CardNumber = cardNumber,
            UserId = userId,
            Status = CardStatus.Active,
            SkipValidatePlate = false
        };

        var user = new User
        {
            Id = userId,
            FullName = "Nguyễn Văn Cư Dân",
            PlateNumber = plateNumber,
            Room = "P502",
            Status = UserStatus.Active
        };

        _cardRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        using var imageStream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF });
        _snapshotStorageMock
            .Setup(s => s.SaveSnapshotAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("snapshots/2026/10/24/test.jpg");

        _ocrServiceMock
            .Setup(o => o.RecognizePlateAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OcrResultDto { PlateNumber = "59X1-123.45", Confidence = 0.95 });

        ParkingSession? savedSession = null;
        _sessionRepositoryMock
            .Setup(s => s.AddAsync(It.IsAny<ParkingSession>(), It.IsAny<CancellationToken>()))
            .Callback<ParkingSession, CancellationToken>((s, _) => savedSession = s)
            .ReturnsAsync((ParkingSession s, CancellationToken _) => s);

        var command = new SwipeCardCommand(
            CardNumber: cardNumber,
            Direction: AccessDirection.In,
            ImageStream: imageStream,
            ImageFileName: "cam_in.jpg",
            LaneCode: "LANE_IN_01");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Decision.Should().Be(AccessDecision.Allowed);
        result.ReasonCode.Should().Be("VALID");
        result.CardNumber.Should().Be(cardNumber);
        result.DetectedPlate.Should().Be("59X1-123.45");
        result.ExpectedPlate.Should().Be(plateNumber);
        result.PlateSimilarityPercent.Should().Be(100.0);
        result.UserFullName.Should().Be("Nguyễn Văn Cư Dân");
        result.UserRoom.Should().Be("P502");
        result.SnapshotPath.Should().Be("snapshots/2026/10/24/test.jpg");

        savedSession.Should().NotBeNull();
        savedSession!.CardNumber.Should().Be(cardNumber);
        savedSession.Status.Should().Be(SessionStatus.Inside);
        savedSession.EntryTime.Should().Be(_fixedNow);
        savedSession.EntrySnapshotPath.Should().Be("snapshots/2026/10/24/test.jpg");

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDirectionIsOut_WithActiveInsideSession_ShouldCloseSessionAndCalculateDuration()
    {
        // Arrange
        const string cardNumber = "0012345678";
        const string plateNumber = "59-X1 123.45";
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var entryTime = _fixedNow.AddHours(-2); // Đỗ 2 tiếng trước

        var card = new Card
        {
            Id = cardId,
            CardNumber = cardNumber,
            UserId = userId,
            Status = CardStatus.Active
        };

        var user = new User
        {
            Id = userId,
            FullName = "Nguyễn Văn Cư Dân",
            PlateNumber = plateNumber,
            Status = UserStatus.Active
        };

        var activeSession = new ParkingSession
        {
            Id = sessionId,
            CardNumber = cardNumber,
            CardId = cardId,
            UserId = userId,
            PlateNumber = plateNumber,
            EntryTime = entryTime,
            Status = SessionStatus.Inside
        };

        _cardRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        using var imageStream = new MemoryStream(new byte[] { 0x01 });
        _ocrServiceMock
            .Setup(o => o.RecognizePlateAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OcrResultDto { PlateNumber = plateNumber });

        _sessionRepositoryMock
            .Setup(s => s.FirstOrDefaultAsync(It.IsAny<Expression<Func<ParkingSession, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeSession);

        var command = new SwipeCardCommand(
            CardNumber: cardNumber,
            Direction: AccessDirection.Out,
            ImageStream: imageStream);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Decision.Should().Be(AccessDecision.Allowed);
        result.ReasonCode.Should().Be("VALID");
        result.SessionId.Should().Be(sessionId);

        activeSession.Status.Should().Be(SessionStatus.Exited);
        activeSession.ExitTime.Should().Be(_fixedNow);
        activeSession.DurationMinutes.Should().Be(120); // 2 hours = 120 mins

        _sessionRepositoryMock.Verify(s => s.UpdateAsync(activeSession, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCardHasSkipValidatePlateTrue_EvenIfPlateMismatches_ShouldAllowAccess()
    {
        // Arrange
        const string cardNumber = "0108394918";
        var card = new Card
        {
            Id = Guid.NewGuid(),
            CardNumber = cardNumber,
            Status = CardStatus.Active,
            SkipValidatePlate = true // Cờ bỏ qua kiểm tra biển số
        };

        _cardRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        using var imageStream = new MemoryStream(new byte[] { 0x01 });
        _ocrServiceMock
            .Setup(o => o.RecognizePlateAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OcrResultDto { PlateNumber = "COMPLETELY-DIFFERENT" });

        var command = new SwipeCardCommand(
            CardNumber: cardNumber,
            Direction: AccessDirection.In,
            ImageStream: imageStream);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Decision.Should().Be(AccessDecision.Allowed);
        result.ReasonCode.Should().Be("SKIP_VALIDATE_ALLOWED");
        result.SkipValidatePlate.Should().BeTrue();

        _sessionRepositoryMock.Verify(s => s.AddAsync(It.IsAny<ParkingSession>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPlateMismatchedAndSkipValidateIsFalse_ShouldReturnDeniedAndNotCreateSession()
    {
        // Arrange
        const string cardNumber = "0012345678";
        var card = new Card
        {
            Id = Guid.NewGuid(),
            CardNumber = cardNumber,
            UserId = Guid.NewGuid(),
            Status = CardStatus.Active,
            SkipValidatePlate = false
        };

        var user = new User
        {
            Id = card.UserId.Value,
            FullName = "Nguyễn Văn A",
            PlateNumber = "59-X1 123.45",
            Status = UserStatus.Active
        };

        _cardRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(card.UserId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        using var imageStream = new MemoryStream(new byte[] { 0x01 });
        _ocrServiceMock
            .Setup(o => o.RecognizePlateAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OcrResultDto { PlateNumber = "29A-888.88" }); // Saigon vs Hanoi plate mismatch

        var command = new SwipeCardCommand(
            CardNumber: cardNumber,
            Direction: AccessDirection.In,
            ImageStream: imageStream);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Decision.Should().Be(AccessDecision.Denied);
        result.ReasonCode.Should().Be("PLATE_MISMATCH");
        result.PlateSimilarityPercent.Should().BeLessThan(80.0);

        _sessionRepositoryMock.Verify(s => s.AddAsync(It.IsAny<ParkingSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOcrIsFalse_ShouldNotCallOcrService_AndStillSaveSnapshot()
    {
        // Arrange
        const string cardNumber = "0108394918";
        var card = new Card
        {
            Id = Guid.NewGuid(),
            CardNumber = cardNumber,
            Status = CardStatus.Active,
            SkipValidatePlate = true
        };

        _cardRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        using var imageStream = new MemoryStream(new byte[] { 0xAA, 0xBB });
        _snapshotStorageMock
            .Setup(s => s.SaveSnapshotAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("snapshots/test_no_ocr.jpg");

        AccessEvent? savedEvent = null;
        _accessEventRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<AccessEvent>(), It.IsAny<CancellationToken>()))
            .Callback<AccessEvent, CancellationToken>((ev, _) => savedEvent = ev)
            .ReturnsAsync((AccessEvent ev, CancellationToken _) => ev);

        ParkingSession? savedSession = null;
        _sessionRepositoryMock
            .Setup(s => s.AddAsync(It.IsAny<ParkingSession>(), It.IsAny<CancellationToken>()))
            .Callback<ParkingSession, CancellationToken>((s, _) => savedSession = s)
            .ReturnsAsync((ParkingSession s, CancellationToken _) => s);

        var command = new SwipeCardCommand(
            CardNumber: cardNumber,
            Direction: AccessDirection.In,
            ImageStream: imageStream,
            Ocr: false); // CỜ OCR = FALSE và Thẻ SkipValidatePlate = TRUE

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Decision.Should().Be(AccessDecision.Allowed);
        result.ReasonCode.Should().Be("SKIP_VALIDATE_ALLOWED");
        result.SnapshotPath.Should().Be("snapshots/test_no_ocr.jpg");

        // KHÔNG ĐƯỢC GỌI OCR SERVICE
        _ocrServiceMock.Verify(o => o.RecognizePlateAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        // NHƯNG VẪN PHẢI LƯU FILE ẢNH SNAPSHOT
        _snapshotStorageMock.Verify(s => s.SaveSnapshotAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        // Kiểm tra lưu path vào cả AccessEvent và ParkingSession
        savedEvent.Should().NotBeNull();
        savedEvent!.SnapshotPath.Should().Be("snapshots/test_no_ocr.jpg");
        savedEvent.SkipValidatePlate.Should().BeTrue();

        savedSession.Should().NotBeNull();
        savedSession!.EntrySnapshotPath.Should().Be("snapshots/test_no_ocr.jpg");
    }

    [Fact]
    public async Task Handle_WhenOcrIsFalse_AndManualPlateProvided_ShouldValidateAgainstManualPlate()
    {
        // Arrange
        const string cardNumber = "0012345678";
        const string registeredPlate = "59-X1 123.45";
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var card = new Card
        {
            Id = cardId,
            CardNumber = cardNumber,
            UserId = userId,
            Status = CardStatus.Active,
            SkipValidatePlate = false
        };

        var user = new User
        {
            Id = userId,
            FullName = "Nguyễn Văn B",
            PlateNumber = registeredPlate,
            Status = UserStatus.Active
        };

        _cardRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Card, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new SwipeCardCommand(
            CardNumber: cardNumber,
            Direction: AccessDirection.In,
            Ocr: false,
            PlateNumber: "59X1-123.45"); // TRUYỀN BIỂN SỐ TRỰC TIẾP

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Decision.Should().Be(AccessDecision.Allowed);
        result.ReasonCode.Should().Be("VALID");
        result.PlateSimilarityPercent.Should().Be(100.0);

        _ocrServiceMock.Verify(o => o.RecognizePlateAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
