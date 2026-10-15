# 🅿️ Core Parking Service

A robust backend service for **Parking Management System**, built with C# .NET 8 following Clean Architecture principles.

## 📐 Architecture

```
core-parking-service/
├── src/
│   ├── ParkingService.Api/            # Presentation Layer (Controllers, Middleware)
│   ├── ParkingService.Application/    # Application Layer (Use Cases, DTOs, Interfaces)
│   ├── ParkingService.Domain/         # Domain Layer (Entities, Enums, Exceptions)
│   └── ParkingService.Infrastructure/ # Infrastructure Layer (EF Core, Repositories)
├── tests/
│   └── ParkingService.Tests/          # Unit & Integration Tests
├── CoreParkingService.sln
├── BUSINESS.md                        # Business Requirements Documentation
└── README.md
```

### Layer Dependencies
```
Api → Application → Domain
Api → Infrastructure → Application → Domain
```

## 🛠️ Tech Stack

| Technology | Purpose |
|------------|---------|
| .NET 8 | Runtime & Framework |
| Entity Framework Core 8 | ORM & Data Access |
| MediatR | CQRS & Mediator Pattern |
| FluentValidation | Input Validation |
| AutoMapper | Object Mapping |
| Serilog | Structured Logging |
| Swagger/OpenAPI | API Documentation |
| xUnit + Moq + FluentAssertions | Testing |

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB or full instance)

### Run the Project
```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/ParkingService.Api
```

### Run Tests
```bash
dotnet test
```

### Swagger UI
Navigate to `https://localhost:5001/swagger` after starting the application.

## 📦 API Response Format

All API responses follow a standardized format:

### ✅ Success Response
```json
{
  "success": true,
  "data": { },
  "error": null
}
```

### ❌ Error Response
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable error message",
    "details": { }
  }
}
```

> `data` and `error` can be an object or `null`.

## 📝 License

Private - All rights reserved.
