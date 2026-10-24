using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Access.DTOs;

/// <summary>
/// DTO thể hiện sự kiện quẹt thẻ kiểm soát xe.
/// </summary>
public class AccessEventDto
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public Guid? CardId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserFullName { get; set; }
    public string? UserRoom { get; set; }
    public AccessDirection Direction { get; set; }
    public string? LaneCode { get; set; }
    public string? OcrPlateNumber { get; set; }
    public string? PlateNormalized { get; set; }
    public string? ExpectedPlateNumber { get; set; }
    public double? PlateSimilarityPercent { get; set; }
    public bool SkipValidatePlate { get; set; }
    public AccessDecision Decision { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string? SnapshotPath { get; set; }
    public DateTime EventTime { get; set; }
    public DateTime CreatedAt { get; set; }
}
