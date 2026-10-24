using System;
using System.Text.Json.Serialization;

namespace CoreParking.Application.Features.Cards.DTOs;

/// <summary>
/// DTO truyền tải thông tin thẻ xe trong kho và cư dân đang sở hữu.
/// </summary>
public record CardDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("cardNumber")] string? CardNumber,
    [property: JsonPropertyName("userId")] Guid? UserId,
    [property: JsonPropertyName("type")] int Type,
    [property: JsonPropertyName("status")] int Status,
    [property: JsonPropertyName("userFullName")] string? UserFullName,
    [property: JsonPropertyName("skipValidatePlate")] bool SkipValidatePlate = false
);
