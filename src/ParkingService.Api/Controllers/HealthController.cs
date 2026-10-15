using Microsoft.AspNetCore.Mvc;

namespace ParkingService.Api.Controllers;

public class HealthController : BaseApiController
{
    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet]
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
