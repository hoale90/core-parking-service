using MediatR;
using Microsoft.AspNetCore.Mvc;
using ParkingService.Application.Common.Models;

namespace ParkingService.Api.Controllers;

/// <summary>
/// Base controller providing standardized response helpers.
/// All controllers should inherit from this.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Returns a standardized success response with data.
    /// </summary>
    protected IActionResult OkResponse<T>(T data)
    {
        return Ok(ApiResponse<T>.SuccessResponse(data));
    }

    /// <summary>
    /// Returns a standardized success response without data.
    /// </summary>
    protected IActionResult OkResponse()
    {
        return Ok(ApiResponse.SuccessResponse());
    }

    /// <summary>
    /// Returns a standardized created response.
    /// </summary>
    protected IActionResult CreatedResponse<T>(T data)
    {
        return StatusCode(StatusCodes.Status201Created, ApiResponse<T>.SuccessResponse(data));
    }

    /// <summary>
    /// Returns a standardized error response.
    /// </summary>
    protected IActionResult ErrorResponse(int statusCode, string code, string message, object? details = null)
    {
        return StatusCode(statusCode, ApiResponse.FailureResponse(code, message, details));
    }
}
