using ParkingService.Domain.Common;
using ParkingService.Domain.Enums;

namespace ParkingService.Domain.Entities;

/// <summary>
/// User entity representing a registered person / staff / resident in the parking management system.
/// Compatible with schema definitions from data.sql.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public int? RoleId { get; set; }
    public string? Role { get; set; }
    public string? Room { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? PlateNumber { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;

    // Navigation property
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
