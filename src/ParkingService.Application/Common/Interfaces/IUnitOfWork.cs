namespace ParkingService.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern interface.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
