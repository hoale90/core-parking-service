# 🤖 AGENTS.md — Quy Định & Hướng Dẫn Phát Triển Core Parking Service

> **Dành cho tất cả AI Coding Agents (Antigravity, Cursor, Copilot, Claude, Gemini)**  
> Mọi thay đổi mã nguồn, tính năng mới, hoặc sửa lỗi trong repository này **BẮT BUỘC** phải tuân thủ nghiêm ngặt các quy tắc dưới đây.

---

## 1. 🎯 Tổng Quan & Phạm Vi Dự Án

- **Dự án**: Core Parking Service (Hệ thống quản lý bãi đỗ xe thông minh).
- **Nền tảng**: C# / .NET 8, ASP.NET Core Web API.
- **Kiến trúc**: Clean Architecture (Domain, Application, Infrastructure, Api).
- **Database**: SQLite với Entity Framework Core 8 (theo `parking_management_requirements.md`).
- **Phạm vi giai đoạn 1 (Phase 1 Scope)**:
  - Chỉ tập trung vào hai nghiệp vụ chính: **User** (Người dùng) và **Card** (Thẻ gửi xe).
  - Các API ban đầu yêu cầu:
    - `POST /api/users` — Thêm người dùng
    - `GET /api/users` — Lấy danh sách người dùng (hỗ trợ phân trang)
    - `GET /api/users/{id}` — Lấy chi tiết người dùng
    - `POST /api/cards` — Thêm thẻ (cho phép `userId = null`)
    - `GET /api/cards` — Lấy danh sách thẻ
    - `GET /api/cards/{id}` — Lấy chi tiết thẻ
  - ⚠️ **Tuyệt đối KHÔNG tự ý suy diễn hoặc thêm các nghiệp vụ phức tạp ngoài scope khi chưa có yêu cầu.**

---

## 2. 📐 Quy Chuẩn Kiến Trúc (Clean Architecture)

Dự án được chia thành 4 layer độc lập với luồng phụ thuộc một chiều (Dependency Inversion):

```
Api (Presentation) 
   │
   ├──▶ Application (Use cases, DTOs, CQRS Handlers, Validators)
   │       │
   │       └──▶ Domain (Entities, Enums, Value Objects, Domain Exceptions)
   │
   └──▶ Infrastructure (EF Core, Repositories, Database, External APIs)
           │
           └──▶ Application & Domain
```

### 2.1. Domain Layer (`ParkingService.Domain`)
- **KHÔNG phụ thuộc vào bất kỳ layer nào khác** và không chứa package bên ngoài (trừ abstractions tối thiểu).
- Mọi entity kế thừa từ [`BaseEntity`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/src/ParkingService.Domain/Common/BaseEntity.cs):
  - Khóa chính `Id` là `Guid` (lưu dạng `TEXT` trong SQLite).
  - Thời gian: `CreatedAt`, `UpdatedAt` luôn lưu chuẩn **UTC** (`DateTime.UtcNow`).
  - Hỗ trợ soft-delete thông qua cờ `IsDeleted` (mặc định `false`).
- Custom exceptions: [`DomainException`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/src/ParkingService.Domain/Exceptions/DomainException.cs), [`NotFoundException`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/src/ParkingService.Domain/Exceptions/NotFoundException.cs), [`ValidationException`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/src/ParkingService.Domain/Exceptions/ValidationException.cs).

### 2.2. Application Layer (`ParkingService.Application`)
- Chứa toàn bộ use-cases theo mô hình CQRS (MediatR `IRequest<T>`, `IRequestHandler<TRequest, TResponse>`).
- Request validation bằng **FluentValidation** (`AbstractValidator<T>`).
- Tự động validate qua [`ValidationBehavior<TRequest, TResponse>`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/src/ParkingService.Application/Common/Behaviors/ValidationBehavior.cs) trong pipeline MediatR.
- Trả về DTOs riêng cho request/response — **TUYỆT ĐỐI KHÔNG trả trực tiếp EF Core Entities ra ngoài API**.

### 2.3. Infrastructure Layer (`ParkingService.Infrastructure`)
- Quản lý EF Core DbContext (`ApplicationDbContext`), Repositories, Unit of Work, và các external integrations (OCR client, USB device drivers).
- Repository implements [`IRepository<T>`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/src/ParkingService.Application/Common/Interfaces/IRepository.cs), tự động lọc các entity đã bị soft-delete (`!e.IsDeleted`).
- Cấu hình SQLite tương thích: Khóa chính UUID dạng TEXT, foreign key check enabled, migrations tự động.

### 2.4. Api Layer (`ParkingService.Api`)
- Controllers kế thừa [`BaseApiController`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/src/ParkingService.Api/Controllers/BaseApiController.cs).
- Sử dụng [`ExceptionHandlingMiddleware`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/src/ParkingService.Api/Middleware/ExceptionHandlingMiddleware.cs) bắt toàn bộ exception và chuẩn hóa format.
- Serilog structured logging ghi nhận console và file rolling logs.

---

## 3. 📦 Chuẩn Hóa API Response (Strict Contract)

TẤT CẢ các endpoint API bắt buộc phải trả về format JSON chuẩn sau:

### 3.1. Phản Hồi Thành Công (Success)
```json
{
  "success": true,
  "data": { ... },
  "error": null
}
```
*Trường hợp không có dữ liệu trả về:*
```json
{
  "success": true,
  "data": null,
  "error": null
}
```

### 3.2. Phản Hồi Thất Bại (Error)
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "ERROR_CODE",
    "message": "Mô tả lỗi dễ hiểu cho client",
    "details": { ... }
  }
}
```

### 3.3. Bảng Mã Lỗi Chuẩn Hóa
| HTTP Status | Error Code | Khi nào sử dụng |
|-------------|------------|-----------------|
| **400 Bad Request** | `VALIDATION_ERROR` | FluentValidation thất bại (details chứa dictionary field: messages) |
| **400 Bad Request** | `DOMAIN_ERROR` | Vi phạm business logic hoặc domain invariant |
| **404 Not Found** | `NOT_FOUND` | Không tìm thấy entity theo Id / criteria |
| **409 Conflict** | `CONFLICT` | Trùng dữ liệu unique (ví dụ: `CardNumber` đã tồn tại) |
| **401 Unauthorized** | `UNAUTHORIZED` | Thiếu hoặc sai token xác thực |
| **403 Forbidden** | `FORBIDDEN` | Không đủ quyền thực hiện hành động |
| **500 Internal** | `INTERNAL_ERROR` | Lỗi hệ thống chưa được phân loại |

### 3.4. Controller Rules
- **LUÔN** sử dụng các helper methods từ `BaseApiController`:
  - `OkResponse(data)` -> HTTP 200 kèm data
  - `OkResponse()` -> HTTP 200 với data = null
  - `CreatedResponse(data)` -> HTTP 201 kèm data
  - `ErrorResponse(statusCode, code, message, details)` -> trả error tùy chỉnh

---

## 4. 🧪 QUY CHUẨN VIẾT TEST THỰC TẾ, KHÔNG ẢO (No Fake Tests)

> ⚠️ **YÊU CẦU ĐẶC BIỆT**: Test case phải là **TEST THỰC TẾ (Realistic, Production-grade Tests)**, phản ánh đúng logic nghiệp vụ, dữ liệu thực tế và các trường hợp biên. **Tuyệt đối KHÔNG viết test ảo, test chiếu lệ!**

### ❌ Những Gì BỊ CẤM (Anti-patterns):
1. **CẤM** test rỗng hoặc tautological assertions kiểu `Assert.True(true)`, `result.Should().NotBeNull()` mà không kiểm tra giá trị bên trong.
2. **CẤM** mock repository chỉ để trả về dummy object rồi assert lại chính dummy object đó mà không có bất kỳ logic nào được thực thi.
3. **CẤM** viết test chỉ cho getter/setter của POCO class mà không có business logic.
4. **CẤM** bỏ qua các edge cases, negative cases (dữ liệu sai, rỗng, trùng lặp, vi phạm ràng buộc).

### ✅ Những Gì BẮT BUỘC PHẢI CÓ (Realistic Testing Checklist):
1. **Kiểm tra Business Invariants & Constraints thực tế**:
   - Thêm User với email/phone không hợp lệ -> Phải ném `ValidationException` với đúng field error.
   - Thêm Card với `CardNumber` đã tồn tại -> Phải trả về lỗi `CONFLICT` hoặc vi phạm unique index.
   - Thêm Card với `UserId` không tồn tại -> Phải báo lỗi validation hoặc foreign key.
   - Thêm Card với `UserId = null` -> Phải thành công và `UserId` trong DB là null.
   - Soft delete -> `IsDeleted = true`, `UpdatedAt` được cập nhật, query thông thường không trả về record này.
2. **Dữ liệu test thực tế (Realistic Test Fixtures)**:
   - Biển số xe thực tế Việt Nam: `"59-X1 123.45"`, `"29A-888.88"`.
   - Số thẻ thực tế: mã RFID/Mifare 10 số `"0012345678"`.
   - Tên, SĐT, Email hợp lệ theo chuẩn Việt Nam.
3. **Assert chi tiết cấu trúc Response**:
   - `response.Success.Should().BeTrue();`
   - `response.Data.Should().NotBeNull();`
   - `response.Data.Id.Should().NotBeEmpty();`
   - `response.Error.Should().BeNull();`
   - Với lỗi: `response.Success.Should().BeFalse(); response.Error.Code.Should().Be("VALIDATION_ERROR");`
4. **Sử dụng EF Core SQLite In-Memory cho Integration Tests**:
   - Chạy với SQLite in-memory provider để test toàn bộ query EF Core, foreign key, unique constraint và soft-delete filter thực tế.

---

## 5. 🛑 Nguyên Tắc Không Tự Suy Diễn (Theo Requirements)

Tuân thủ nghiêm ngặt Phần 8 của [`parking_management_requirements.md`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/parking_management_requirements.md):

1. **Không tự quyết định quyền vào/ra bằng OCR**: OCR Service chỉ nhận diện biển số và trả kết quả; Core Service mới là nơi quyết định `Allowed` / `Denied`.
2. **Thẻ chưa gán người dùng (`UserId = NULL`)**: Không mặc nhiên được tự động mở barrier.
3. **Lệnh barrier**: Gửi lệnh mở barrier thành công KHÔNG đồng nghĩa với barrier đã mở thực tế trên phần cứng.
4. **Bảo mật**: Tuyệt đối không lưu mật khẩu hoặc cấu hình nhạy cảm dưới dạng plain-text.
5. **Thời gian**: Thống nhất dùng UTC ISO 8601 trong toàn bộ database và API.

---

## 6. 🔄 Quy Trình Triển Khai Tính Năng Mới

Mỗi khi agent implement một tính năng mới (ví dụ: User CRUD):

1. **Domain**: Khai báo Entity kế thừa `BaseEntity`, tạo Enums nếu cần.
2. **Infrastructure**: Tạo EF Core EntityTypeConfiguration (bảng SQLite, column types, unique index, soft-delete query filter), cập nhật DbContext.
3. **Application**:
   - Tạo Command/Query + Request DTO + Response DTO.
   - Tạo FluentValidation Validator cho Command.
   - Viết Handler xử lý nghiệp vụ qua `IRepository<T>` và `IUnitOfWork`.
4. **Api**: Tạo Controller kế thừa `BaseApiController`, gọi MediatR, trả về `OkResponse(data)` / `CreatedResponse(data)`.
5. **Tests**:
   - Viết Validator Tests (kiểm tra rules: rỗng, quá dài, định dạng sai).
   - Viết Handler Tests (kiểm tra happy path, duplicate path, not found path).
   - Viết API / Contract Tests (kiểm tra chuẩn hóa response `{ success, data, error }`).
6. **Chạy kiểm thử & Verify**: Đảm bảo toàn bộ test case pass và không có warning nghiêm trọng.
