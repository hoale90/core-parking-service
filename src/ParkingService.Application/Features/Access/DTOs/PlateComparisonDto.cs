namespace ParkingService.Application.Features.Access.DTOs;

/// <summary>
/// DTO chứa thông tin phân tích chi tiết so khớp giữa 2 biển số xe.
/// Phục vụ việc kiểm thử, tinh chỉnh % tỷ lệ so khớp.
/// </summary>
public class PlateComparisonDto
{
    public string DetectedPlate { get; set; } = string.Empty;
    public string ExpectedPlate { get; set; } = string.Empty;
    public string NormalizedDetected { get; set; } = string.Empty;
    public string NormalizedExpected { get; set; } = string.Empty;
    public double SimilarityPercent { get; set; }
    public int Distance { get; set; }
    public double ThresholdPercent { get; set; }
    public bool IsMatch { get; set; }
    public string Evaluation { get; set; } = string.Empty;
}
