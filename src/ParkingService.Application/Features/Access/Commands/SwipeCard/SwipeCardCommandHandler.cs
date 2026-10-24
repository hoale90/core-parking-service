using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Access.DTOs;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Access.Commands.SwipeCard;

public class SwipeCardCommandHandler : IRequestHandler<SwipeCardCommand, SwipeCardResultDto>
{
    private const double DefaultPlateMatchThreshold = 80.0;

    private readonly IRepository<Card> _cardRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<AccessEvent> _accessEventRepository;
    private readonly IRepository<ParkingSession> _sessionRepository;
    private readonly IPlateComparisonService _plateComparisonService;
    private readonly ISnapshotStorageService _snapshotStorage;
    private readonly IOcrService _ocrService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SwipeCardCommandHandler(
        IRepository<Card> cardRepository,
        IRepository<User> userRepository,
        IRepository<AccessEvent> accessEventRepository,
        IRepository<ParkingSession> sessionRepository,
        IPlateComparisonService plateComparisonService,
        ISnapshotStorageService snapshotStorage,
        IOcrService ocrService,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _cardRepository = cardRepository;
        _userRepository = userRepository;
        _accessEventRepository = accessEventRepository;
        _sessionRepository = sessionRepository;
        _plateComparisonService = plateComparisonService;
        _snapshotStorage = snapshotStorage;
        _ocrService = ocrService;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<SwipeCardResultDto> Handle(SwipeCardCommand request, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;
        var normalizedCardNumber = request.CardNumber.Trim();
        var threshold = request.CustomThresholdPercent ?? DefaultPlateMatchThreshold;

        // 1. Lưu ảnh chụp snapshot và gọi OCR nhận diện biển số (nếu có truyền ảnh)
        string? snapshotPath = null;
        string? ocrPlate = null;

        if (request.ImageStream != null)
        {
            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                await request.ImageStream.CopyToAsync(memoryStream, cancellationToken);
                imageBytes = memoryStream.ToArray();
            }

            if (imageBytes.Length > 0)
            {
                // Lưu file ảnh snapshot
                try
                {
                    using var saveStream = new MemoryStream(imageBytes);
                    snapshotPath = await _snapshotStorage.SaveSnapshotAsync(
                        saveStream,
                        request.ImageFileName ?? "snapshot.jpg",
                        cancellationToken);
                }
                catch
                {
                    // Tiếp tục xử lý ngay cả khi lưu ảnh thất bại
                }

                // Gửi sang OCR nhận diện biển số
                try
                {
                    using var ocrStream = new MemoryStream(imageBytes);
                    var ocrResult = await _ocrService.RecognizePlateAsync(
                        ocrStream,
                        request.ImageFileName ?? "snapshot.jpg",
                        cancellationToken);

                    if (ocrResult != null && !string.IsNullOrWhiteSpace(ocrResult.PlateNumber))
                    {
                        ocrPlate = ocrResult.PlateNumber.Trim();
                    }
                }
                catch
                {
                    // Bỏ qua lỗi OCR để không làm crash luồng quẹt thẻ
                }
            }
        }

        // 2. Tra cứu thẻ xe trong cơ sở dữ liệu
        var card = await _cardRepository.FirstOrDefaultAsync(
            c => c.CardNumber.ToLower() == normalizedCardNumber.ToLower(),
            cancellationToken);

        // Chuẩn bị các biến kết quả
        var decision = AccessDecision.Denied;
        string reasonCode;
        string reasonMessage;
        string? expectedPlate = null;
        double? similarityPercent = null;
        string? normPlate = _plateComparisonService.Normalize(ocrPlate);
        User? linkedUser = null;

        if (card == null)
        {
            reasonCode = "CARD_NOT_FOUND";
            reasonMessage = $"Mã thẻ {normalizedCardNumber} không tồn tại trong hệ thống.";
        }
        else if (card.Status != CardStatus.Active)
        {
            reasonCode = "CARD_INACTIVE";
            reasonMessage = $"Thẻ {card.CardNumber} đang ở trạng thái không hoạt động (Inactive/Bị khóa).";
        }
        else
        {
            // Thẻ tồn tại và Active -> Kiểm tra người dùng liên kết
            if (card.UserId.HasValue)
            {
                linkedUser = await _userRepository.GetByIdAsync(card.UserId.Value, cancellationToken);
                expectedPlate = linkedUser?.PlateNumber;
            }

            if (linkedUser != null && linkedUser.Status == UserStatus.Disabled)
            {
                reasonCode = "USER_DISABLED";
                reasonMessage = $"Cư dân {linkedUser.FullName} đang bị tạm khóa quyền sử dụng bãi xe.";
            }
            else if (card.SkipValidatePlate)
            {
                // Thẻ có cấu hình bỏ qua kiểm tra biển số
                decision = AccessDecision.Allowed;
                reasonCode = "SKIP_VALIDATE_ALLOWED";
                reasonMessage = "Thẻ được phép qua barrier (Đã cấu hình bỏ qua đối chiếu biển số xe).";

                if (!string.IsNullOrWhiteSpace(ocrPlate) && !string.IsNullOrWhiteSpace(expectedPlate))
                {
                    var comp = _plateComparisonService.Compare(ocrPlate, expectedPlate, threshold);
                    similarityPercent = comp.SimilarityPercent;
                }
            }
            else
            {
                // Bắt buộc phải đối chiếu biển số xe
                if (linkedUser == null || string.IsNullOrWhiteSpace(expectedPlate))
                {
                    reasonCode = "NO_USER_ASSIGNED";
                    reasonMessage = $"Thẻ {card.CardNumber} chưa được liên kết với cư dân hoặc chưa đăng ký biển số xe.";
                }
                else if (string.IsNullOrWhiteSpace(ocrPlate))
                {
                    reasonCode = "OCR_FAILED";
                    reasonMessage = "Không nhận diện được biển số xe từ camera. Yêu cầu kiểm tra vị trí xe hoặc ảnh chụp.";
                }
                else
                {
                    // So khớp biển số xe thông minh bằng thuật toán Levenshtein
                    var comparison = _plateComparisonService.Compare(ocrPlate, expectedPlate, threshold);
                    similarityPercent = comparison.SimilarityPercent;

                    if (comparison.IsMatch)
                    {
                        decision = AccessDecision.Allowed;
                        reasonCode = "VALID";
                        reasonMessage = $"Xác thực thành công. Biển số khớp {comparison.SimilarityPercent}% (Ngưỡng {threshold}%).";
                    }
                    else
                    {
                        decision = AccessDecision.Denied;
                        reasonCode = "PLATE_MISMATCH";
                        reasonMessage = $"Biển số xe không khớp ({comparison.SimilarityPercent}% < ngưỡng {threshold}%). Xe nhận diện: {ocrPlate}, Xe đăng ký: {expectedPlate}.";
                    }
                }
            }
        }

        // 3. Ghi bản ghi vào AccessEvents (Audit Log)
        var accessEvent = new AccessEvent
        {
            Id = Guid.NewGuid(),
            CardNumber = normalizedCardNumber,
            CardId = card?.Id,
            UserId = card?.UserId,
            Direction = request.Direction,
            LaneCode = request.LaneCode,
            OcrPlateNumber = ocrPlate,
            PlateNormalized = normPlate,
            ExpectedPlateNumber = expectedPlate,
            PlateSimilarityPercent = similarityPercent,
            SkipValidatePlate = card?.SkipValidatePlate ?? false,
            Decision = decision,
            ReasonCode = reasonCode,
            SnapshotPath = snapshotPath,
            EventTime = now,
            CreatedAt = now,
            IsDeleted = false
        };

        await _accessEventRepository.AddAsync(accessEvent, cancellationToken);

        // 4. Quản lý chu trình phiên gửi xe (ParkingSessions) khi được phép qua (Allowed)
        Guid? sessionId = null;
        if (decision == AccessDecision.Allowed && card != null)
        {
            if (request.Direction == AccessDirection.In)
            {
                var newSession = new ParkingSession
                {
                    Id = Guid.NewGuid(),
                    CardNumber = card.CardNumber,
                    CardId = card.Id,
                    UserId = card.UserId,
                    PlateNumber = ocrPlate ?? expectedPlate,
                    EntryTime = now,
                    EntryEventId = accessEvent.Id,
                    EntrySnapshotPath = snapshotPath,
                    Status = SessionStatus.Inside,
                    CreatedAt = now,
                    IsDeleted = false
                };

                await _sessionRepository.AddAsync(newSession, cancellationToken);
                sessionId = newSession.Id;
            }
            else if (request.Direction == AccessDirection.Out)
            {
                // Tìm phiên xe đang đỗ bên trong bãi gần nhất
                var activeSession = await _sessionRepository.FirstOrDefaultAsync(
                    s => s.CardNumber.ToLower() == card.CardNumber.ToLower() && s.Status == SessionStatus.Inside,
                    cancellationToken);

                if (activeSession != null)
                {
                    activeSession.Status = SessionStatus.Exited;
                    activeSession.ExitTime = now;
                    activeSession.ExitEventId = accessEvent.Id;
                    activeSession.ExitSnapshotPath = snapshotPath;
                    activeSession.DurationMinutes = (int)Math.Max(0, (now - activeSession.EntryTime).TotalMinutes);
                    activeSession.UpdatedAt = now;

                    await _sessionRepository.UpdateAsync(activeSession, cancellationToken);
                    sessionId = activeSession.Id;
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SwipeCardResultDto
        {
            Decision = decision,
            ReasonCode = reasonCode,
            ReasonMessage = reasonMessage,
            CardNumber = normalizedCardNumber,
            Direction = request.Direction,
            LaneCode = request.LaneCode,
            DetectedPlate = ocrPlate,
            ExpectedPlate = expectedPlate,
            PlateSimilarityPercent = similarityPercent,
            SkipValidatePlate = card?.SkipValidatePlate ?? false,
            UserFullName = linkedUser?.FullName,
            UserRoom = linkedUser?.Room,
            UserPhone = linkedUser?.Phone,
            SessionId = sessionId,
            EventId = accessEvent.Id,
            SnapshotPath = snapshotPath,
            EventTime = now
        };
    }
}
