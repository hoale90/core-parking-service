using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Common.Models;
using ParkingService.Infrastructure.Persistence;
using ParkingService.Infrastructure.Services;

namespace ParkingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=parking_service.db";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) && !connectionString.Contains("Server="))
            {
                options.UseSqlite(connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            }
            else
            {
                options.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            }
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // OCR Service configuration & HttpClient
        services.Configure<OcrSettings>(configuration.GetSection("OcrService"));
        var ocrSettings = configuration.GetSection("OcrService").Get<OcrSettings>() ?? new OcrSettings();

        services.AddHttpClient<IOcrService, OcrService>(client =>
        {
            client.BaseAddress = new Uri(ocrSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(ocrSettings.TimeoutSeconds);
        });

        return services;
    }
}
