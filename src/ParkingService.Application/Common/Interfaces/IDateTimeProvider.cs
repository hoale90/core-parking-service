namespace ParkingService.Application.Common.Interfaces;

/// <summary>
/// Abstraction for DateTime to support testability.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
