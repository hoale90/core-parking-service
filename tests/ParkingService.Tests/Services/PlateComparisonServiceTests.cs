using FluentAssertions;
using ParkingService.Application.Common.Services;
using Xunit;

namespace ParkingService.Tests.Services;

public class PlateComparisonServiceTests
{
    private readonly PlateComparisonService _service = new();

    [Theory]
    [InlineData("59-X1 123.45", "59X112345")]
    [InlineData("29A-888.88", "29A88888")]
    [InlineData("  51f - 999.99  ", "51F99999")]
    [InlineData("43-C1: 567.89", "43C156789")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void Normalize_ShouldRemoveSpecialCharactersSpacesAndConvertToUppercase(string? input, string expected)
    {
        // Act
        var result = _service.Normalize(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("59X112345", "59X112345", 0)]
    [InlineData("59X112345", "59X112346", 1)] // 1 char difference
    [InlineData("59X112345", "59X11234", 1)]  // 1 char missing
    [InlineData("ABC", "XYZ", 3)]
    public void CalculateLevenshteinDistance_ShouldReturnExactEditDistance(string a, string b, int expectedDistance)
    {
        // Act
        var distance = _service.CalculateLevenshteinDistance(a, b);

        // Assert
        distance.Should().Be(expectedDistance);
    }

    [Fact]
    public void Compare_WhenPlatesAreIdentical_ShouldReturn100PercentAndMatch()
    {
        // Arrange
        const string detected = "59-X1 123.45";
        const string registered = "59X1-123.45";

        // Act
        var result = _service.Compare(detected, registered, thresholdPercent: 80.0);

        // Assert
        result.IsMatch.Should().BeTrue();
        result.SimilarityPercent.Should().Be(100.0);
        result.Distance.Should().Be(0);
        result.NormalizedDetected.Should().Be("59X112345");
        result.NormalizedExpected.Should().Be("59X112345");
    }

    [Fact]
    public void Compare_WhenOcrHasMinorTypo_AboveThreshold_ShouldBeMatch()
    {
        // Arrange: 9 chars, 1 typo ('B' misread as '8') -> 8/9 = 88.89% >= 80%
        const string detected = "298-888.88"; // OCR misread 'A' as '8'
        const string registered = "29A-888.88";

        // Act
        var result = _service.Compare(detected, registered, thresholdPercent: 80.0);

        // Assert
        result.Distance.Should().Be(1);
        result.SimilarityPercent.Should().Be(87.5); // maxLen = 8 ("29888888"), (1 - 1/8) = 87.5%
        result.IsMatch.Should().BeTrue();
    }

    [Fact]
    public void Compare_WhenPlatesAreCompletelyDifferent_ShouldNotMatch()
    {
        // Arrange: Saigon plate vs Hanoi plate
        const string detected = "59-X1 123.45";
        const string registered = "29A-888.88";

        // Act
        var result = _service.Compare(detected, registered, thresholdPercent: 80.0);

        // Assert
        result.IsMatch.Should().BeFalse();
        result.SimilarityPercent.Should().BeLessThan(50.0);
    }

    [Fact]
    public void Compare_WhenDetectedPlateIsEmpty_ShouldReturnZeroPercentAndNotMatch()
    {
        // Arrange
        const string? detected = null;
        const string registered = "59-X1 123.45";

        // Act
        var result = _service.Compare(detected, registered, thresholdPercent: 80.0);

        // Assert
        result.IsMatch.Should().BeFalse();
        result.SimilarityPercent.Should().Be(0.0);
    }
}
