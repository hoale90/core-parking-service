# 📋 Core Parking Service - Business & Implementation Documentation

> **Dự án**: Core Parking Service (Hệ thống quản lý bãi đỗ xe thông minh)  
> **Nền tảng**: C# / .NET 8, ASP.NET Core Web API, SQLite with EF Core 8  
> **Cập nhật lần cuối**: 2026-10-15  
> **Trạng thái**: ✅ Giai đoạn 1 (Phase 1) hoàn thành: User, Card, SQLite Migration & OCR Integration

---

## 1. 🎯 Tổng Quan & Phạm Vi Giai Đoạn 1 (Phase 1 Scope)

Theo tài liệu [parking_management_requirements.md](file:///D:/Projects/parking-manager/parking-service/core-parking-service/parking_management_requirements.md), giai đoạn 1 tập trung triển khai:
1. **Kết nối Database**: SQLite với EF Core 8, tự động chạy migration khi khởi động ứng dụng.
2. **Cấu hình OCR Service**: Tích hợp với Python OCR Service qua HTTP Client (`/health`, `/ocr`).
3. **Quản lý Người dùng (User Management)**:
   - Entity `User`: `Id`, `FullName`, `DateOfBirth`, `Gender`, `Phone`, `Email`, `Status` (Active, Disabled), `CreatedAt`, `UpdatedAt`.
4. **Quản lý Thẻ gửi xe (Card Management)**:
   - Entity `Card`: `Id`, `CardNumber` (unique), `UserId` (nullable), `Status` (Active, Blocked, Lost), `CreatedAt`, `UpdatedAt`.
   - `Card.UserId` cho phép `NULL` (hỗ trợ thẻ chưa gán người dùng hoặc thẻ khách).
   - Ràng buộc: `CardNumber` duy nhất (không cho phép 2 thẻ trùng số); không cho gán thẻ vào `UserId` không tồn tại.
5. **Chuẩn hóa API Response**: Mọi endpoint trả về định dạng `{ success, data, error }`.

---

## 2. 🗄️ Cấu Hình Database & OCR Service

### 2.1. `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=parking_service.db"
  },
  "OcrService": {
    "BaseUrl": "http://localhost:8000",
    "HealthEndpoint": "/health",
    "OcrEndpoint": "/ocr",
    "TimeoutSeconds": 10
  }
}
```

### 2.2. Tự Động Migration Khi Khởi Chạy
Khi ứng dụng khởi động trong `Program.cs`, DbContext tự động gọi:
```csharp
context.Database.Migrate();
```
Tập tin migration ban đầu: `20261015070000_InitialCreate.cs` sẽ tự động tạo bảng `Users`, `Cards`, các khóa ngoại và chỉ mục duy nhất `IX_Cards_CardNumber`.

---

## 3. 🚀 Hướng Dẫn Chạy & Kiểm Thử Ứng Dụng

### 3.1. Yêu Cầu Môi Trường
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Python 3.10+ (nếu chạy OCR Service)

### 3.2. Lệnh Chạy
```bash
# 1. Restore các dependencies
dotnet restore

# 2. Build toàn bộ solution
dotnet build

# 3. Chạy toàn bộ bộ test thực tế (Unit & Contract Tests)
dotnet test

# 4. Khởi chạy Web API (tự động tạo database SQLite)
dotnet run --project src/ParkingService.Api
```

### 3.3. Truy Cập Swagger UI
Sau khi khởi chạy ứng dụng, truy cập:
`https://localhost:5001/swagger` hoặc `http://localhost:5000/swagger`

---

## 4. 📡 Danh Sách API & Ví Dụ Request/Response

### 4.1. `POST /api/users` — Thêm Người Dùng

#### Request:
```http
POST /api/users
Content-Type: application/json

{
  "fullName": "Nguyễn Văn Nam",
  "dateOfBirth": "1992-06-15T00:00:00Z",
  "gender": "Nam",
  "phone": "0912345678",
  "email": "nam.nguyen@example.com",
  "status": 1
}
```

#### Response Thành Công (HTTP 201 Created):
```json
{
  "success": true,
  "data": {
    "id": "e4b6c3f1-2852-4e94-82a1-63cb535a0921",
    "fullName": "Nguyễn Văn Nam",
    "dateOfBirth": "1992-06-15T00:00:00Z",
    "gender": "Nam",
    "phone": "0912345678",
    "email": "nam.nguyen@example.com",
    "status": 1,
    "createdAt": "2026-10-15T14:15:30.1234567Z",
    "updatedAt": null
  },
  "error": null
}
```

#### Response Lỗi Validation (HTTP 400 Bad Request):
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred.",
    "details": {
      "FullName": ["Họ và tên không được để trống."],
      "Email": ["Địa chỉ email không đúng định dạng."]
    }
  }
}
```

---

### 4.2. `GET /api/users` — Danh Sách Người Dùng (Phân Trang)

#### Request:
```http
GET /api/users?pageNumber=1&pageSize=10&searchTerm=Nam
```

#### Response (HTTP 200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "e4b6c3f1-2852-4e94-82a1-63cb535a0921",
        "fullName": "Nguyễn Văn Nam",
        "dateOfBirth": "1992-06-15T00:00:00Z",
        "gender": "Nam",
        "phone": "0912345678",
        "email": "nam.nguyen@example.com",
        "status": 1,
        "createdAt": "2026-10-15T14:15:30.1234567Z",
        "updatedAt": null
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "error": null
}
```

---

### 4.3. `GET /api/users/{id}` — Chi Tiết Người Dùng

#### Request:
```http
GET /api/users/e4b6c3f1-2852-4e94-82a1-63cb535a0921
```

#### Response Thành Công (HTTP 200 OK):
```json
{
  "success": true,
  "data": {
    "id": "e4b6c3f1-2852-4e94-82a1-63cb535a0921",
    "fullName": "Nguyễn Văn Nam",
    "dateOfBirth": "1992-06-15T00:00:00Z",
    "gender": "Nam",
    "phone": "0912345678",
    "email": "nam.nguyen@example.com",
    "status": 1,
    "createdAt": "2026-10-15T14:15:30.1234567Z",
    "updatedAt": null
  },
  "error": null
}
```

#### Response Không Tìm Thấy (HTTP 404 Not Found):
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "NOT_FOUND",
    "message": "Entity \"User\" (e4b6c3f1-2852-4e94-82a1-63cb535a0921) was not found.",
    "details": null
  }
}
```

---

### 4.4. `POST /api/cards` — Thêm Thẻ Gửi Xe

#### Request (Thẻ chưa gán người dùng - `userId = null`):
```http
POST /api/cards
Content-Type: application/json

{
  "cardNumber": "0012345678",
  "userId": null,
  "status": 1
}
```

#### Response Thành Công (HTTP 201 Created):
```json
{
  "success": true,
  "data": {
    "id": "a1d7f8e2-4567-4a12-9c34-88aa11bb22cc",
    "cardNumber": "0012345678",
    "userId": null,
    "status": 1,
    "createdAt": "2026-10-15T14:20:00.0000000Z",
    "updatedAt": null
  },
  "error": null
}
```

#### Response Lỗi Trùng Mã Thẻ (HTTP 409 Conflict):
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "CONFLICT",
    "message": "Card with CardNumber '0012345678' already exists.",
    "details": null
  }
}
```

#### Response Lỗi Gán Cho `UserId` Không Tồn Tại (HTTP 404 Not Found):
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "NOT_FOUND",
    "message": "Entity \"User\" (00000000-0000-0000-0000-000000000000) was not found.",
    "details": null
  }
}
```

---

### 4.5. `GET /api/cards` — Danh Sách Thẻ (Phân Trang & Lọc)

#### Request:
```http
GET /api/cards?pageNumber=1&pageSize=10&status=1
```

#### Response (HTTP 200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "a1d7f8e2-4567-4a12-9c34-88aa11bb22cc",
        "cardNumber": "0012345678",
        "userId": null,
        "status": 1,
        "createdAt": "2026-10-15T14:20:00.0000000Z",
        "updatedAt": null
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "error": null
}
```

---

### 4.6. `GET /api/cards/{id}` — Chi Tiết Thẻ

#### Request:
```http
GET /api/cards/a1d7f8e2-4567-4a12-9c34-88aa11bb22cc
```

#### Response (HTTP 200 OK):
```json
{
  "success": true,
  "data": {
    "id": "a1d7f8e2-4567-4a12-9c34-88aa11bb22cc",
    "cardNumber": "0012345678",
    "userId": null,
    "status": 1,
    "createdAt": "2026-10-15T14:20:00.0000000Z",
    "updatedAt": null
  },
  "error": null
}
```

---

## 5. 🛑 Nguyên Tắc Không Tự Suy Diễn (Theo Requirements)

1. **Không tự quyết định quyền vào/ra bằng OCR**: OCR chỉ đọc biển số, Core Service mới ra quyết định `Allowed` / `Denied`.
2. **Thẻ chưa gán người dùng (`UserId = NULL`)**: Không mặc nhiên được tự động mở barrier.
3. **Lệnh barrier**: Gửi lệnh thành công không đồng nghĩa phần cứng barrier đã mở.
4. **Bảo mật**: Tuyệt đối không lưu mật khẩu hoặc cấu hình nhạy cảm dưới dạng plain-text.
5. **Thời gian**: Thống nhất dùng UTC ISO 8601 trong toàn bộ database và API.
