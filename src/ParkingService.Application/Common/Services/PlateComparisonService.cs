using System.Text.RegularExpressions;
using ParkingService.Application.Common.Interfaces;

namespace ParkingService.Application.Common.Services;

/// <summary>
/// Cài đặt thuật toán so khớp biển số xe thông minh:
/// 1. Chuẩn hóa biển số: Loại bỏ mọi ký tự không phải chữ và số (Regex: [^A-Z0-9]), đưa về chữ in hoa.
/// 2. Đo khoảng cách Levenshtein giữa 2 chuỗi biển số đã chuẩn hóa.
/// 3. Tính tỷ lệ tương đồng: Similarity% = (1 - Distance / MaxLen) * 100.
/// 4. Đánh giá IsMatch dựa trên ngưỡng cấu hình (mặc định 80%).
/// </summary>
public partial class PlateComparisonService : IPlateComparisonService
{
    private static readonly Regex NonAlphanumericRegex = MyRegex();

    public string Normalize(string? plate)
    {
        if (string.IsNullOrWhiteSpace(plate))
        {
            return string.Empty;
        }

        // Chuyển in hoa và loại bỏ mọi ký tự không phải chữ số hoặc chữ cái tiếng Anh
        var upper = plate.Trim().ToUpperInvariant();
        return NonAlphanumericRegex.Replace(upper, string.Empty);
    }

    public int CalculateLevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a))
        {
            return string.IsNullOrEmpty(b) ? 0 : b.Length;
        }

        if (string.IsNullOrEmpty(b))
        {
            return a.Length;
        }

        var lenA = a.Length;
        var lenB = b.Length;
        var matrix = new int[lenA + 1, lenB + 1];

        for (var i = 0; i <= lenA; i++)
        {
            matrix[i, 0] = i;
        }

        for (var j = 0; j <= lenB; j++)
        {
            matrix[0, j] = j;
        }

        for (var i = 1; i <= lenA; i++)
        {
            for (var j = 1; j <= lenB; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost
                );
            }
        }

        return matrix[lenA, lenB];
    }

    public PlateComparisonResult Compare(string? detectedPlate, string? expectedPlate, double thresholdPercent = 80.0)
    {
        var normDetected = Normalize(detectedPlate);
        var normExpected = Normalize(expectedPlate);

        // Trường hợp cả hai đều rỗng
        if (string.IsNullOrEmpty(normDetected) && string.IsNullOrEmpty(normExpected))
        {
            return new PlateComparisonResult(
                IsMatch: true,
                SimilarityPercent: 100.0,
                NormalizedDetected: string.Empty,
                NormalizedExpected: string.Empty,
                Distance: 0,
                ThresholdPercent: thresholdPercent
            );
        }

        // Một trong hai chuỗi rỗng
        if (string.IsNullOrEmpty(normDetected) || string.IsNullOrEmpty(normExpected))
        {
            var maxL = Math.Max(normDetected.Length, normExpected.Length);
            return new PlateComparisonResult(
                IsMatch: false,
                SimilarityPercent: 0.0,
                NormalizedDetected: normDetected,
                NormalizedExpected: normExpected,
                Distance: maxL,
                ThresholdPercent: thresholdPercent
            );
        }

        // Cả 2 giống hệt nhau
        if (normDetected == normExpected)
        {
            return new PlateComparisonResult(
                IsMatch: true,
                SimilarityPercent: 100.0,
                NormalizedDetected: normDetected,
                NormalizedExpected: normExpected,
                Distance: 0,
                ThresholdPercent: thresholdPercent
            );
        }

        var distance = CalculateLevenshteinDistance(normDetected, normExpected);
        var maxLen = Math.Max(normDetected.Length, normExpected.Length);
        var similarity = Math.Max(0.0, Math.Round((1.0 - (double)distance / maxLen) * 100.0, 2));

        var isMatch = similarity >= thresholdPercent;

        return new PlateComparisonResult(
            IsMatch: isMatch,
            SimilarityPercent: similarity,
            NormalizedDetected: normDetected,
            NormalizedExpected: normExpected,
            Distance: distance,
            ThresholdPercent: thresholdPercent
        );
    }

    [GeneratedRegex(@"[^A-Z0-9]")]
    private static partial Regex MyRegex();
}
