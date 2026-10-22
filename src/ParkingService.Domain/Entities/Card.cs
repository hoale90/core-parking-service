using ParkingService.Domain.Common;
using ParkingService.Domain.Enums;

namespace ParkingService.Domain.Entities;

/// <summary>
/// Card entity representing an RFID/Mifare access card.
/// UserId can be null to represent unassigned or guest cards.
/// Compatible with schema definitions from data.sql.
/// </summary>
public class Card : BaseEntity
{
    public string CardNumber { get; set; } = string.Empty;
    public CardType Type { get; set; } = CardType.Registered;
    public string? VehiclePlate { get; set; }
    public bool SkipValidateVehiclePlate { get; set; } = false;
    public string? Info { get; set; }
    public string? Phone { get; set; }
    public string? Room { get; set; }
    public Guid? UserId { get; set; }
    public CardStatus Status { get; set; } = CardStatus.Active;

    // Navigation property
    public User? User { get; set; }
}
