using ParkingService.Domain.Common;
using ParkingService.Domain.Enums;

namespace ParkingService.Domain.Entities;

/// <summary>
/// Card entity representing an RFID/Mifare access card.
/// UserId can be null to represent unassigned or guest cards.
/// </summary>
public class Card : BaseEntity
{
    public string CardNumber { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public CardStatus Status { get; set; } = CardStatus.Active;

    // Navigation property
    public User? User { get; set; }
}
