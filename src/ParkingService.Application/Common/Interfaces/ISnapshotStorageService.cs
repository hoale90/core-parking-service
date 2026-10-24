namespace ParkingService.Application.Common.Interfaces;

/// <summary>
/// Dịch vụ lưu trữ hình ảnh snapshot chụp từ camera tại cổng ra/vào.
/// </summary>
public interface ISnapshotStorageService
{
    /// <summary>
    /// Lưu trữ file ảnh và trả về đường dẫn tương đối để lưu vào cơ sở dữ liệu.
    /// Định dạng lưu trữ: snapshots/YYYY/MM/DD/{guid}_{filename}.jpg
    /// </summary>
    Task<string> SaveSnapshotAsync(Stream stream, string originalFileName, CancellationToken cancellationToken = default);
}
