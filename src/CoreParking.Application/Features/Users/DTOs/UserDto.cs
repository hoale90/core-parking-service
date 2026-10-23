using System;
using System.Text.Json.Serialization;

namespace CoreParking.Application.Features.Users.DTOs;

/// <summary>
/// DTO truyá»n táº£i thÃ´ng tin cÆ° dÃ¢n vÃ  tháº» xe Ä‘Æ°á»£c gÃ¡n, khá»›p 100% vá»›i UserManagementHttpService.
/// </summary>
public record UserDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("fullName")] string? FullName,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("plateNumber")] string? PlateNumber,
    [property: JsonPropertyName("room")] string? Room,
    [property: JsonPropertyName("linkedCardId")] Guid? LinkedCardId,
    [property: JsonPropertyName("linkedCardNumber")] string? LinkedCardNumber
);