using Microsoft.AspNetCore.Mvc;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Access.Commands.SwipeCard;
using ParkingService.Application.Features.Access.DTOs;
using ParkingService.Application.Features.Access.Queries.GetAccessEvents;
using ParkingService.Application.Features.Access.Queries.GetActiveSessions;
using ParkingService.Application.Features.Access.Queries.TestPlateComparison;
using ParkingService.Domain.Enums;

namespace ParkingService.Api.Controllers;

/// <summary>
/// Model nhận request quẹt thẻ từ ứng dụng cổng kiểm soát / Kiosk.
/// Hỗ trợ upload ảnh chụp trực tiếp từ camera qua multipart/form-data.
/// </summary>
public class SwipeCardApiRequest
{
    /// <summary>
    /// Mã số in hoặc đọc từ thẻ RFID/Mifare (bắt buộc).
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Hướng quẹt thẻ: "In" / 1 (Vào) hoặc "Out" / 2 (Ra). Mặc định là In.
    /// </summary>
    public AccessDirection Direction { get; set; } = AccessDirection.In;

    /// <summary>
    /// Mã định danh làn kiểm soát (ví dụ: LANE_IN_01, LANE_OUT_01).
    /// </summary>
    public string? LaneCode { get; set; }

    /// <summary>
    /// Tệp hình ảnh chụp xe từ camera làn (JPEG, PNG).
    /// </summary>
    public IFormFile? Image { get; set; }

    /// <summary>
    /// Cờ kích hoạt gọi Python OCR Service nhận diện biển số từ ảnh:
    /// - true: Tự động gửi ảnh sang Python OCR Service để đọc biển số.
    /// - false: Không gọi OCR Service.
    /// </summary>
    public bool Ocr { get; set; } = true;

    /// <summary>
    /// Biển số xe truyền trực tiếp (tùy chọn: dùng khi camera/client đã tự nhận diện sẵn hoặc khi Ocr = false).
    /// </summary>
    public string? PlateNumber { get; set; }

    /// <summary>
    /// Ngưỡng phần trăm so khớp biển số tùy chỉnh (0.0 -> 100.0). Nếu bỏ trống sẽ dùng cấu hình hệ thống (mặc định 80%).
    /// </summary>
    public double? CustomThresholdPercent { get; set; }
}

public record TestPlateComparisonApiRequest(
    string PlateA,
    string PlateB,
    double ThresholdPercent = 80.0
);

/// <summary>
/// Controller xử lý nghiệp vụ quẹt thẻ, nhận diện biển số OCR, mở barrier và quản lý lịch sử ra/vào.
/// </summary>
[Tags("Access Control & Barrier")]
public class AccessController : BaseApiController
{
    /// <summary>
    /// Quẹt thẻ tại cổng kiểm soát: Nhận mã thẻ + ảnh chụp camera, tự động nhận diện biển số (nếu ocr = true),
    /// so khớp thông tin, ghi nhận lịch sử và trả về quyết định Allowed hoặc Denied mở barrier.
    /// </summary>
    /// <param name="request">Dữ liệu quẹt thẻ gồm CardNumber, Direction, Image, LaneCode, Ocr</param>
    /// <returns>Quyết định mở barrier kèm lý do và thông tin phân tích</returns>
    [HttpPost("swipe")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<SwipeCardResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SwipeCard([FromForm] SwipeCardApiRequest request)
    {
        Stream? stream = null;
        if (request.Image != null && request.Image.Length > 0)
        {
            stream = request.Image.OpenReadStream();
        }

        var command = new SwipeCardCommand(
            CardNumber: request.CardNumber,
            Direction: request.Direction,
            ImageStream: stream,
            ImageFileName: request.Image?.FileName,
            LaneCode: request.LaneCode,
            Ocr: request.Ocr,
            PlateNumber: request.PlateNumber,
            CustomThresholdPercent: request.CustomThresholdPercent
        );

        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Kiểm thử và tinh chỉnh thuật toán so khớp 2 biển số xe với ngưỡng % tương đồng tùy chọn.
    /// </summary>
    /// <param name="request">2 biển số cần đối chiếu và ngưỡng %</param>
    /// <returns>Báo cáo chi tiết: khoảng cách Levenshtein, độ tương đồng % và kết quả IsMatch</returns>
    [HttpPost("test-plate-comparison")]
    [ProducesResponseType(typeof(ApiResponse<PlateComparisonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestPlateComparison([FromBody] TestPlateComparisonApiRequest request)
    {
        var query = new TestPlateComparisonQuery(request.PlateA, request.PlateB, request.ThresholdPercent);
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }

    /// <summary>
    /// Lấy danh sách lịch sử quẹt thẻ và các lượt ra/vào cổng (phân trang và bộ lọc).
    /// </summary>
    /// <param name="query">Bộ lọc tìm kiếm theo mã thẻ, hướng, kết quả, thời gian</param>
    /// <returns>Danh sách phân trang sự kiện AccessEvents</returns>
    [HttpGet("events")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<AccessEventDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents([FromQuery] GetAccessEventsQuery query)
    {
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }

    /// <summary>
    /// Lấy danh sách xe hiện đang gửi bên trong bãi (Status = Inside).
    /// </summary>
    /// <param name="query">Bộ lọc tìm kiếm và phân trang</param>
    /// <returns>Danh sách phân trang các phiên xe đang đỗ trong bãi</returns>
    [HttpGet("sessions/inside")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ParkingSessionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveSessions([FromQuery] GetActiveSessionsQuery query)
    {
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }
}
