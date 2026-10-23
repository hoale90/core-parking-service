using System;
using System.Text.Json.Serialization;

namespace CoreParking.Application.Features.Cards.DTOs;

/// <summary>
/// DTO truyá»n táº£i thÃ´ng tin tháº» xe trong kho vÃ  cÆ° dÃ¢n Ä‘ang sá»Ÿ há»¯u.
/// </summary>
public record CardDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("cardNumber")] string? CardNumber,
    [property: JsonPropertyName("userId")] Guid? UserId,
    [property: JsonPropertyName("type")] int Type,
    [property: JsonPropertyName("status")] int Status,
    [property: JsonPropertyName("userFullName")] string? UserFullName
);