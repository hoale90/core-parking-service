using Microsoft.EntityFrameworkCore;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Domain.Entities;

namespace ParkingService.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<AccessEvent> AccessEvents => Set<AccessEvent>();
    public DbSet<ParkingSession> ParkingSessions => Set<ParkingSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
