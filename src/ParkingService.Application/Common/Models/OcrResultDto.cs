using System.Text.Json.Serialization;

namespace ParkingService.Application.Common.Models;

/// <summary>
/// Data transfer object returned by the OCR Service.
/// Corresponds to Section 4.3 and 4.4 of requirements.
/// </summary>
public class OcrResultDto
{
    [JsonPropertyName("plate_number")]
    public string? PlateNumber { get; set; }

    [JsonPropertyName("plate_display")]
    public string? PlateDisplay { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("processing_ms")]
    public int ProcessingMs { get; set; }
}
