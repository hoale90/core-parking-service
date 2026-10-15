using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Cards.DTOs;

/// <summary>
/// Card response DTO. Encapsulates Card entity.
/// </summary>
public class CardDto
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public CardStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
