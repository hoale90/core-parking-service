using FluentAssertions;
using ParkingService.Application.Common.Models;
using Xunit;

namespace ParkingService.Tests;

public class ApiResponseTests
{
    [Fact]
    public void SuccessResponse_WithData_ShouldReturnCorrectStructure()
    {
        // Arrange
        var data = new { id = 1, name = "Test" };

        // Act
        var response = ApiResponse<object>.SuccessResponse(data);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Error.Should().BeNull();
    }

    [Fact]
    public void SuccessResponse_WithoutData_ShouldReturnCorrectStructure()
    {
        // Act
        var response = ApiResponse.SuccessResponse();

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().BeNull();
        response.Error.Should().BeNull();
    }

    [Fact]
    public void FailureResponse_ShouldReturnCorrectStructure()
    {
        // Act
        var response = ApiResponse.FailureResponse("ERR_001", "Something went wrong");

        // Assert
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Error.Should().NotBeNull();
        response.Error!.Code.Should().Be("ERR_001");
        response.Error.Message.Should().Be("Something went wrong");
    }

    [Fact]
    public void FailureResponse_WithDetails_ShouldIncludeDetails()
    {
        // Arrange
        var details = new { field = "email", reason = "invalid format" };

        // Act
        var response = ApiResponse.FailureResponse("VALIDATION_ERROR", "Validation failed", details);

        // Assert
        response.Success.Should().BeFalse();
        response.Error.Should().NotBeNull();
        response.Error!.Details.Should().NotBeNull();
    }
}
