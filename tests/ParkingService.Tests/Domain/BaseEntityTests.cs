using FluentAssertions;
using ParkingService.Domain.Common;
using Xunit;

namespace ParkingService.Tests.Domain;

// Concrete implementation of BaseEntity for testing domain logic
public class TestEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

public class BaseEntityTests
{
    [Fact]
    public void NewEntity_ShouldInitializeWithUniqueIdAndUtcTimestamp()
    {
        // Arrange & Act
        var entity1 = new TestEntity { Name = "Entity 1" };
        var entity2 = new TestEntity { Name = "Entity 2" };

        // Assert - Real ID uniqueness
        entity1.Id.Should().NotBeEmpty();
        entity2.Id.Should().NotBeEmpty();
        entity1.Id.Should().NotBe(entity2.Id);

        // Assert - Realistic UTC timestamp within recent range
        entity1.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        entity1.UpdatedAt.Should().BeNull();
        entity1.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void SoftDelete_ShouldMarkEntityAsDeleted()
    {
        // Arrange
        var entity = new TestEntity { Name = "Active Entity" };

        // Act
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = "admin-user";

        // Assert
        entity.IsDeleted.Should().BeTrue();
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        entity.UpdatedBy.Should().Be("admin-user");
    }
}
