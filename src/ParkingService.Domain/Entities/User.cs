using ParkingService.Domain.Common;
using ParkingService.Domain.Enums;

namespace ParkingService.Domain.Entities;

/// <summary>
/// User entity representing a registered person in the parking management system.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;

    // Navigation property
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
