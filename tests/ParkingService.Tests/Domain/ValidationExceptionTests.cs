using FluentAssertions;
using ParkingService.Domain.Exceptions;
using Xunit;

namespace ParkingService.Tests.Domain;

public class ValidationExceptionTests
{
    [Fact]
    public void Constructor_WithPropertyNameAndMessage_ShouldPopulateErrorsDictionary()
    {
        // Arrange
        const string propertyName = "PlateNumber";
        const string errorMessage = "Biển số xe không được để trống theo quy định.";

        // Act
        var exception = new ValidationException(propertyName, errorMessage);

        // Assert
        exception.Message.Should().Be("One or more validation errors occurred.");
        exception.Errors.Should().ContainKey(propertyName);
        exception.Errors[propertyName].Should().ContainSingle().Which.Should().Be(errorMessage);
    }

    [Fact]
    public void Constructor_WithMultipleValidationFailures_ShouldAggregateCorrectly()
    {
        // Arrange
        var failures = new Dictionary<string, string[]>
        {
            { "CardNumber", new[] { "CardNumber must be unique.", "CardNumber format is invalid." } },
            { "UserId", new[] { "Referenced User does not exist." } }
        };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Errors.Should().HaveCount(2);
        exception.Errors["CardNumber"].Should().HaveCount(2);
        exception.Errors["CardNumber"].Should().Contain("CardNumber must be unique.");
        exception.Errors["UserId"].Should().ContainSingle().Which.Should().Be("Referenced User does not exist.");
    }
}
