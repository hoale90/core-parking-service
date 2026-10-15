using System.Text.Json.Serialization;

namespace ParkingService.Application.Common.Models;

/// <summary>
/// Standardized API response wrapper.
/// All API responses follow this structure:
/// { "success": true/false, "data": {}/null, "error": {}/null }
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("error")]
    public ApiError? Error { get; set; }

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Error = null
        };
    }

    /// <summary>
    /// Creates a successful response without data.
    /// </summary>
    public static ApiResponse<T> SuccessResponse()
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = default,
            Error = null
        };
    }

    /// <summary>
    /// Creates a failure response with error details.
    /// </summary>
    public static ApiResponse<T> FailureResponse(string code, string message, object? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Error = new ApiError
            {
                Code = code,
                Message = message,
                Details = details
            }
        };
    }
}

/// <summary>
/// Non-generic version for responses without typed data.
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public new static ApiResponse SuccessResponse()
    {
        return new ApiResponse
        {
            Success = true,
            Data = null,
            Error = null
        };
    }

    public new static ApiResponse FailureResponse(string code, string message, object? details = null)
    {
        return new ApiResponse
        {
            Success = false,
            Data = null,
            Error = new ApiError
            {
                Code = code,
                Message = message,
                Details = details
            }
        };
    }
}
