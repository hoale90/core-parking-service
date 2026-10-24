namespace ParkingService.Application.Common.Interfaces;

/// <summary>
/// Kết quả so khớp giữa hai biển số xe.
/// </summary>
public record PlateComparisonResult(
    bool IsMatch,
    double SimilarityPercent,
    string NormalizedDetected,
    string NormalizedExpected,
    int Distance,
    double ThresholdPercent
);

/// <summary>
/// Dịch vụ so khớp biển số xe (Fuzzy License Plate Matching) dựa trên thuật toán khoảng cách Levenshtein.
/// </summary>
public interface IPlateComparisonService
{
    /// <summary>
    /// Chuẩn hóa chuỗi biển số (viết hoa, loại bỏ khoảng trắng, dấu gạch ngang, dấu chấm và ký tự đặc biệt).
    /// Ví dụ: "59-X1 123.45" -> "59X112345"
    /// </summary>
    string Normalize(string? plate);

    /// <summary>
    /// Tính toán khoảng cách Levenshtein giữa hai chuỗi ký tự.
    /// </summary>
    int CalculateLevenshteinDistance(string a, string b);

    /// <summary>
    /// So khớp 2 biển số xe theo tỷ lệ phần trăm tương đồng (0% - 100%).
    /// </summary>
    /// <param name="detectedPlate">Biển số OCR nhận diện được</param>
    /// <param name="expectedPlate">Biển số xe đã đăng ký của cư dân</param>
    /// <param name="thresholdPercent">Ngưỡng phần trăm tối thiểu để tính là khớp (ví dụ: 80.0)</param>
    /// <returns>Kết quả so khớp chi tiết</returns>
    PlateComparisonResult Compare(string? detectedPlate, string? expectedPlate, double thresholdPercent = 80.0);
}
