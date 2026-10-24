using MediatR;
using ParkingService.Application.Features.Access.DTOs;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Access.Commands.SwipeCard;

/// <summary>
/// Lệnh quẹt thẻ xe tại cổng barrier:
/// - Xác thực mã thẻ và người dùng
/// - Chụp và lưu ảnh snapshot
/// - Gọi OCR nhận diện biển số
/// - Kiểm tra cờ SkipValidatePlate và so khớp biển số với ngưỡng %
/// - Ghi nhận lịch sử AccessEvents và cập nhật ParkingSessions
/// - Trả về quyết định Allowed hoặc Denied
/// </summary>
public record SwipeCardCommand(
    string CardNumber,
    AccessDirection Direction,
    Stream? ImageStream = null,
    string? ImageFileName = null,
    string? LaneCode = null,
    double? CustomThresholdPercent = null
) : IRequest<SwipeCardResultDto>;
