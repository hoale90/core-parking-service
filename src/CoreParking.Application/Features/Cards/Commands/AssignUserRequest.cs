using System;
using System.Text.Json.Serialization;

namespace CoreParking.Application.Features.Cards.Commands;

/// <summary>
/// Payload cho API gÃ¡n hoáº·c há»§y gÃ¡n tháº»: PUT /api/cards/{cardId}/assign-user
/// Náº¿u UserId = null: Thao tÃ¡c há»§y gÃ¡n (thu há»“i vá» kho).
/// </summary>
public record AssignUserRequest
{
    [JsonPropertyName("userId")]
    public Guid? UserId { get; init; }
}