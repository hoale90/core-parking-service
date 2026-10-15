# Quy Chuẩn Clean Architecture & Code Style

## Nguyên Tắc Phụ Thuộc (Dependency Inversion)
- Mã nguồn phải luôn tuân thủ nguyên tắc Clean Architecture. Luồng phụ thuộc:
  - `ParkingService.Domain`: Không tham chiếu bất kỳ project nội bộ nào, không tham chiếu thư viện ngoài ngoại trừ các abstraction cơ bản.
  - `ParkingService.Application`: Chỉ tham chiếu `ParkingService.Domain`.
  - `ParkingService.Infrastructure`: Tham chiếu `ParkingService.Application` và `ParkingService.Domain`.
  - `ParkingService.Api`: Tham chiếu `ParkingService.Application` và `ParkingService.Infrastructure`.
  - `ParkingService.Tests`: Tham chiếu cả 3 layer để viết unit & integration tests.

## Cấu Trúc Thư Mục Chuẩn
```
src/
├── ParkingService.Domain/
│   ├── Common/               # BaseEntity, IAuditableEntity
│   ├── Entities/             # User, Card, Vehicle, Lane, etc.
│   ├── Enums/                # UserStatus, CardStatus, VehicleType, etc.
│   └── Exceptions/           # DomainException, NotFoundException, ValidationException
├── ParkingService.Application/
│   ├── Common/
│   │   ├── Behaviors/        # MediatR Pipeline Behaviors (ValidationBehavior)
│   │   ├── Interfaces/       # IRepository, IUnitOfWork, IDateTimeProvider
│   │   └── Models/           # ApiResponse, ApiError, PaginatedResult
│   └── Features/             # Thư mục theo tính năng (Users/Commands/..., Users/Queries/...)
├── ParkingService.Infrastructure/
│   ├── Persistence/          # ApplicationDbContext, Configurations, Repositories
│   └── Services/             # DateTimeProvider, external service clients
└── ParkingService.Api/
    ├── Controllers/          # BaseApiController, UsersController, CardsController
    └── Middleware/           # ExceptionHandlingMiddleware
```

## Quy Tắc Đặt Tên & Triển Khai
- CQRS Pattern:
  - Command: `CreateUserCommand`, `UpdateCardStatusCommand`
  - Query: `GetUserByIdQuery`, `GetCardsQuery`
  - Handler: `CreateUserCommandHandler`, `GetUserByIdQueryHandler`
  - Validator: `CreateUserCommandValidator` (kế thừa `AbstractValidator<CreateUserCommand>`)
- Tất cả các tác vụ bất đồng bộ đều có `CancellationToken cancellationToken = default`.
- Đăng ký Dependency Injection thông qua các extension method `AddApplicationServices` và `AddInfrastructureServices`.
