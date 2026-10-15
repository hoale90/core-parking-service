using Microsoft.AspNetCore.Mvc;
using ParkingService.Application.Common.Models;

namespace ParkingService.Api.Controllers;

/// <summary>
/// Controller kiểm tra trạng thái hoạt động của dịch vụ (Health Check).
/// </summary>
public class HealthController : BaseApiController
{
    /// <summary>
    /// Kiểm tra trạng thái hoạt động của hệ thống Core Parking Service.
    /// </summary>
    /// <returns>Trạng thái dịch vụ, thời gian UTC hiện tại, và phiên bản hệ thống</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var healthData = new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        };

        return OkResponse(healthData);
    }
}
