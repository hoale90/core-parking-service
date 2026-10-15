using System.Net;
using System.Text.Json;
using ParkingService.Application.Common.Models;
using ParkingService.Domain.Exceptions;

namespace ParkingService.Api.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches all exceptions and returns standardized API responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, message, details) = exception switch
        {
            ValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                "VALIDATION_ERROR",
                validationEx.Message,
                (object?)validationEx.Errors),

            ConflictException conflictEx => (
                (int)HttpStatusCode.Conflict,
                "CONFLICT",
                conflictEx.Message,
                (object?)null),

            NotFoundException notFoundEx => (
                (int)HttpStatusCode.NotFound,
                "NOT_FOUND",
                notFoundEx.Message,
                (object?)null),

            DomainException domainEx => (
                (int)HttpStatusCode.BadRequest,
                "DOMAIN_ERROR",
                domainEx.Message,
                (object?)null),

            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                "UNAUTHORIZED",
                "You are not authorized to perform this action.",
                (object?)null),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                "An unexpected error occurred. Please try again later.",
                (object?)null)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse.FailureResponse(errorCode, message, details);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
