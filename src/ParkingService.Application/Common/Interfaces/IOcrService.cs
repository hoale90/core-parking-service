using ParkingService.Application.Common.Models;

namespace ParkingService.Application.Common.Interfaces;

/// <summary>
/// Abstraction for communicating with the external OCR Service.
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Checks health and readiness of the OCR Service.
    /// </summary>
    Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an image to the OCR service to recognize license plate.
    /// </summary>
    Task<OcrResultDto?> RecognizePlateAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);
}
