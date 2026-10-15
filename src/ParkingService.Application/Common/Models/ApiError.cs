using System.Text.Json.Serialization;

namespace ParkingService.Application.Common.Models;

/// <summary>
/// Standardized error object in API responses.
/// </summary>
public class ApiError
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public object? Details { get; set; }
}
