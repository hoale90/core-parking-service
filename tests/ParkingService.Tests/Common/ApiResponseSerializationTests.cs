using System.Text.Json;
using FluentAssertions;
using ParkingService.Application.Common.Models;
using Xunit;

namespace ParkingService.Tests.Common;

public class ApiResponseSerializationTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    [Fact]
    public void SuccessResponse_ShouldSerializeWithExactRequiredContract()
    {
        // Arrange
        var testData = new { userId = "U12345", cardNumber = "0841295731", status = "Active" };
        var response = ApiResponse<object>.SuccessResponse(testData);

        // Act
        var json = JsonSerializer.Serialize(response, _jsonOptions);

        // Assert - Real JSON contract matching user specification
        json.Should().Contain("\"success\":true");
        json.Should().Contain("\"data\":{");
        json.Should().Contain("\"cardNumber\":\"0841295731\"");
        json.Should().Contain("\"error\":null");
    }

    [Fact]
    public void FailureResponse_ShouldSerializeWithStandardizedErrorDetails()
    {
        // Arrange
        var validationErrors = new Dictionary<string, string[]>
        {
            { "CardNumber", new[] { "Card number already exists in system." } }
        };
        var response = ApiResponse.FailureResponse("CONFLICT", "Card number duplicate detected.", validationErrors);

        // Act
        var json = JsonSerializer.Serialize(response, _jsonOptions);

        // Assert - Real error envelope structure
        json.Should().Contain("\"success\":false");
        json.Should().Contain("\"data\":null");
        json.Should().Contain("\"error\":{");
        json.Should().Contain("\"code\":\"CONFLICT\"");
        json.Should().Contain("\"message\":\"Card number duplicate detected.\"");
        json.Should().Contain("\"CardNumber\":[\"Card number already exists in system.\"]");
    }
}
