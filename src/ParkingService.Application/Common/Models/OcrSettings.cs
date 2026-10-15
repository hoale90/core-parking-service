namespace ParkingService.Application.Common.Models;

/// <summary>
/// Configuration settings for connecting to the OCR Service.
/// </summary>
public class OcrSettings
{
    public string BaseUrl { get; set; } = "http://localhost:8000";
    public string HealthEndpoint { get; set; } = "/health";
    public string OcrEndpoint { get; set; } = "/ocr";
    public int TimeoutSeconds { get; set; } = 10;
}
