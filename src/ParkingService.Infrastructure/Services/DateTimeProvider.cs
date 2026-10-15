using ParkingService.Application.Common.Interfaces;

namespace ParkingService.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
