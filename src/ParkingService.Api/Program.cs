using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using ParkingService.Api.Middleware;
using ParkingService.Application;
using ParkingService.Infrastructure;
using ParkingService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "🅿️ Core Parking Service API",
        Version = "v1",
        Description = """
        ### Hệ Thống Quản Lý Bãi Đỗ Xe Thông Minh (Core Parking Service)

        API backend hỗ trợ quản lý người dùng, thẻ gửi xe RFID/Mifare, tích hợp OCR nhận diện biển số và điều khiển barrier.

        #### 📦 Chuẩn hóa phản hồi API:
        Mọi endpoint trả về định dạng JSON thống nhất:
        ```json
        {
          "success": true,
          "data": { ... },
          "error": null
        }
        ```
        Khi xảy ra lỗi:
        ```json
        {
          "success": false,
          "data": null,
          "error": {
            "code": "VALIDATION_ERROR | NOT_FOUND | CONFLICT | DOMAIN_ERROR | INTERNAL_ERROR",
            "message": "Thông điệp lỗi chi tiết",
            "details": { ... }
          }
        }
        ```

        #### 📋 Phạm vi Giai đoạn 1:
        - **Users**: Thêm, lấy danh sách, lấy chi tiết người dùng.
        - **Cards**: Thêm thẻ (hỗ trợ `userId = null`), danh sách, chi tiết thẻ (ràng buộc `CardNumber` duy nhất).
        """,
        Contact = new OpenApiContact
        {
            Name = "Core Parking Service Team",
            Url = new Uri("https://github.com/hoale90/core-parking-service")
        }
    });

    // Tích hợp XML Comments từ Api và Application layers
    var xmlFileApi = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPathApi = Path.Combine(AppContext.BaseDirectory, xmlFileApi);
    if (File.Exists(xmlPathApi))
    {
        options.IncludeXmlComments(xmlPathApi, includeControllerXmlComments: true);
    }

    var xmlFileApp = "ParkingService.Application.xml";
    var xmlPathApp = Path.Combine(AppContext.BaseDirectory, xmlFileApp);
    if (File.Exists(xmlPathApp))
    {
        options.IncludeXmlComments(xmlPathApp);
    }
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto-migrate SQLite Database on application start
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        Log.Information("Applying EF Core migrations to SQLite database...");
        context.Database.Migrate();
        Log.Information("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable Swagger UI for interactive testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Core Parking Service API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Parking Service API Documentation & Testing";
    c.DisplayRequestDuration();
    c.DocExpansion(DocExpansion.List);
    c.EnableFilter();
    c.EnableDeepLinking();
});

// Redirect root to Swagger UI for quick testing
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

Log.Information("Parking Service API starting...");
app.Run();
