using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Access.DTOs;

/// <summary>
/// DTO thể hiện thông tin một phiên gửi xe.
/// </summary>
public class ParkingSessionDto
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public Guid? CardId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserFullName { get; set; }
    public string? UserRoom { get; set; }
    public string? PlateNumber { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public int? DurationMinutes { get; set; }
    public string? EntrySnapshotPath { get; set; }
    public string? ExitSnapshotPath { get; set; }
    public SessionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
