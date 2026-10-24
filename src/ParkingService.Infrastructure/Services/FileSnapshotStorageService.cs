using Microsoft.Extensions.Logging;
using ParkingService.Application.Common.Interfaces;

namespace ParkingService.Infrastructure.Services;

/// <summary>
/// Dịch vụ lưu trữ file ảnh snapshot cục bộ vào thư mục snapshots/{YYYY}/{MM}/{DD}/
/// </summary>
public class FileSnapshotStorageService : ISnapshotStorageService
{
    private readonly ILogger<FileSnapshotStorageService> _logger;

    public FileSnapshotStorageService(ILogger<FileSnapshotStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<string> SaveSnapshotAsync(Stream stream, string originalFileName, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(ext))
        {
            ext = ".jpg";
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var relativeDir = Path.Combine("snapshots", now.Year.ToString(), now.Month.ToString("D2"), now.Day.ToString("D2"));
        var baseDir = Directory.GetCurrentDirectory();
        var fullDir = Path.Combine(baseDir, relativeDir);

        if (!Directory.Exists(fullDir))
        {
            Directory.CreateDirectory(fullDir);
        }

        var fullPath = Path.Combine(fullDir, uniqueFileName);
        using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        var relativePath = Path.Combine(relativeDir, uniqueFileName).Replace("\\", "/");
        _logger.LogInformation("Saved snapshot image to {RelativePath}", relativePath);
        return relativePath;
    }
}
