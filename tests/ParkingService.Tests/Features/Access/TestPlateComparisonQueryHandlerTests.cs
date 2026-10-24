using FluentAssertions;
using ParkingService.Application.Common.Services;
using ParkingService.Application.Features.Access.Queries.TestPlateComparison;
using Xunit;

namespace ParkingService.Tests.Features.Access;

public class TestPlateComparisonQueryHandlerTests
{
    private readonly TestPlateComparisonQueryHandler _handler = new(new PlateComparisonService());

    [Fact]
    public async Task Handle_WhenPlatesMatchAboveThreshold_ShouldReturnIsMatchTrueWithDetailedEvaluation()
    {
        // Arrange: "59-X1 123.45" vs "59X1-123.45"
        var query = new TestPlateComparisonQuery("59-X1 123.45", "59X1-123.45", ThresholdPercent: 80.0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsMatch.Should().BeTrue();
        result.SimilarityPercent.Should().Be(100.0);
        result.Distance.Should().Be(0);
        result.NormalizedDetected.Should().Be("59X112345");
        result.NormalizedExpected.Should().Be("59X112345");
        result.Evaluation.Should().Contain("ĐẠT");
    }

    [Fact]
    public async Task Handle_WhenPlatesAreBelowThreshold_ShouldReturnIsMatchFalse()
    {
        // Arrange
        var query = new TestPlateComparisonQuery("59-X1 123.45", "29A-999.99", ThresholdPercent: 80.0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsMatch.Should().BeFalse();
        result.SimilarityPercent.Should().BeLessThan(80.0);
        result.Evaluation.Should().Contain("TỪ CHỐI");
    }
}
