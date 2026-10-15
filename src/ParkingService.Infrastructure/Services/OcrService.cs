using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Common.Models;

namespace ParkingService.Infrastructure.Services;

/// <summary>
/// HTTP client implementation for communicating with the Python OCR Service.
/// </summary>
public class OcrService : IOcrService
{
    private readonly HttpClient _httpClient;
    private readonly OcrSettings _settings;
    private readonly ILogger<OcrService> _logger;

    public OcrService(
        HttpClient httpClient,
        IOptions<OcrSettings> settings,
        ILogger<OcrService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(_settings.HealthEndpoint, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OCR Service health check failed at {Url}", _settings.BaseUrl + _settings.HealthEndpoint);
            return false;
        }
    }

    public async Task<OcrResultDto?> RecognizePlateAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(imageStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(streamContent, "image", fileName);

            var response = await _httpClient.PostAsync(_settings.OcrEndpoint, content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OCR Service returned error HTTP {StatusCode}: {Response}", response.StatusCode, responseString);
                return null;
            }

            var jsonDoc = JsonDocument.Parse(responseString);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
            {
                if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Object)
                {
                    return JsonSerializer.Deserialize<OcrResultDto>(dataProp.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while calling OCR Service at {Url}", _settings.BaseUrl + _settings.OcrEndpoint);
            return null;
        }
    }
}
