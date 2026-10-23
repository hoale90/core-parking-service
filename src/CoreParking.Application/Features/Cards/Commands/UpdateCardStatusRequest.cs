using System.Text.Json.Serialization;

namespace CoreParking.Application.Features.Cards.Commands;

/// <summary>
/// Payload cho API cáº­p nháº­t tráº¡ng thÃ¡i tháº»: PUT /api/cards/{cardId}/status
/// 1: Active, 2: Locked, 3: Lost
/// </summary>
public record UpdateCardStatusRequest
{
    [JsonPropertyName("status")]
    public int Status { get; init; }
}