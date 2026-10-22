# 📋 Core Parking Service - Business & Implementation Documentation

> **Dự án**: Core Parking Service (Hệ thống quản lý bãi đỗ xe thông minh)  
> **Nền tảng**: C# / .NET 8, ASP.NET Core Web API, SQLite with EF Core 8  
> **Cập nhật lần cuối**: 2026-10-22  
> **Trạng thái**: ✅ Giai đoạn 1 (Phase 1) hoàn thành: User & Card tương thích chuẩn `data.sql`, SQLite Migration, OCR Integration

---

## 1. 🎯 Tổng Quan & Phạm Vi Giai Đoạn 1 (Phase 1 Scope)

Theo tài liệu [parking_management_requirements.md](file:///D:/Projects/parking-manager/parking-service/core-parking-service/parking_management_requirements.md) và cấu trúc chuẩn từ `data.sql`:
1. **Kết nối Database**: SQLite với EF Core 8, tự động chạy migration khi khởi động ứng dụng.
2. **Cấu hình OCR Service**: Tích hợp với Python OCR Service qua HTTP Client (`/health`, `/ocr`).
3. **Quản lý Người dùng (User Management)**:
   - Entity `User`:
     - `Id`: Khóa chính Guid
     - `FullName`: Họ và tên đầy đủ
     - `Username`: Tên tài khoản người dùng/nhân viên (từ `data.sql`)
     - `RoleId`: ID vai trò hệ thống (1: technical-admin, 3: admin, 4: staff - từ `data.sql`)
     - `Role`: Tên vai trò hiển thị
     - `Room`: Số phòng / căn hộ cư dân (ví dụ: "B22", "A16")
     - `DateOfBirth`: Ngày sinh (UTC)
     - `Gender`: Giới tính
     - `Phone`: Số điện thoại liên hệ
     - `Email`: Địa chỉ email
     - `PlateNumber`: Số xe / Biển số xe đăng ký
     - `Status`: `Active` (1) hoặc `Disabled` (2)
     - `CreatedAt`, `UpdatedAt`, `IsDeleted`
4. **Quản lý Thẻ gửi xe (Card Management)**:
   - Entity `Card`:
     - `Id`: Khóa chính Guid
     - `CardNumber`: Mã số in hoặc đọc từ thẻ (tương thích `uuid` trong `data.sql`, unique index)
     - `Type`: Loại thẻ (`Registered` = 1, `Temporary` = 2, `Unknown` = 3 - từ `data.sql`)
     - `VehiclePlate`: Biển số xe gắn với thẻ (ví dụ: "38B1-28228", "94L1-05580" - từ `data.sql`)
     - `SkipValidateVehiclePlate`: Cờ bỏ qua đối chiếu biển số khi ra/vào (từ `data.sql`)
     - `Info`: Thông tin chủ thẻ / ghi chú (từ `data.sql`)
     - `Phone`: Số điện thoại liên hệ của chủ thẻ (từ `data.sql`)
     - `Room`: Số phòng / căn hộ của chủ thẻ (từ `data.sql`)
     - `UserId`: ID người dùng (nullable - hỗ trợ thẻ khách hoặc chưa gán người dùng)
     - `Status`: Trạng thái thẻ (`Active` = 1, `Blocked` = 2, `Lost` = 3)
     - `CreatedAt`, `UpdatedAt`, `IsDeleted`
5. **Chuẩn hóa API Response**: Mọi endpoint trả về định dạng chuẩn `{ success, data, error }`.

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
Các migration:
- `20261015070000_InitialCreate.cs`: Khởi tạo bảng `Users`, `Cards`.
- `20261015083000_AddPlateNumberToUsers.cs`: Thêm trường `PlateNumber` vào `Users`.
- `20261022110000_AddDataSqlFieldsToUserAndCard.cs`: Bổ sung các trường chuẩn từ `data.sql` vào `Cards` (`Type`, `VehiclePlate`, `SkipValidateVehiclePlate`, `Info`, `Phone`, `Room`) và `Users` (`Username`, `RoleId`, `Role`, `Room`).

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

# 3. Chạy toàn bộ bộ test thực tế (52 test cases passed)
dotnet test

# 4. Khởi chạy Web API (tự động tạo và cập nhật database SQLite)
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
  "username": "nam.nguyen",
  "roleId": 3,
  "role": "admin",
  "room": "B22",
  "dateOfBirth": "1992-06-15T00:00:00Z",
  "gender": "Nam",
  "phone": "0912345678",
  "email": "nam.nguyen@example.com",
  "plateNumber": "59-X1 123.45",
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
    "username": "nam.nguyen",
    "roleId": 3,
    "role": "admin",
    "room": "B22",
    "dateOfBirth": "1992-06-15T00:00:00Z",
    "gender": "Nam",
    "phone": "0912345678",
    "email": "nam.nguyen@example.com",
    "plateNumber": "59-X1 123.45",
    "status": 1,
    "createdAt": "2026-10-22T10:15:30.1234567Z",
    "updatedAt": null
  },
  "error": null
}
```

---

### 4.2. `GET /api/users` — Danh Sách Người Dùng (Phân Trang & Lọc)

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
        "username": "nam.nguyen",
        "roleId": 3,
        "role": "admin",
        "room": "B22",
        "dateOfBirth": "1992-06-15T00:00:00Z",
        "gender": "Nam",
        "phone": "0912345678",
        "email": "nam.nguyen@example.com",
        "plateNumber": "59-X1 123.45",
        "status": 1,
        "createdAt": "2026-10-22T10:15:30.1234567Z",
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
    "username": "nam.nguyen",
    "roleId": 3,
    "role": "admin",
    "room": "B22",
    "dateOfBirth": "1992-06-15T00:00:00Z",
    "gender": "Nam",
    "phone": "0912345678",
    "email": "nam.nguyen@example.com",
    "plateNumber": "59-X1 123.45",
    "status": 1,
    "createdAt": "2026-10-22T10:15:30.1234567Z",
    "updatedAt": null
  },
  "error": null
}
```

---

### 4.4. `POST /api/cards` — Thêm Thẻ Gửi Xe

#### Request (Thẻ đăng ký căn hộ/nhà trọ theo chuẩn `data.sql`):
```http
POST /api/cards
Content-Type: application/json

{
  "cardNumber": "2848255313",
  "type": 1,
  "vehiclePlate": "94L1-05580",
  "skipValidateVehiclePlate": false,
  "info": "DUONG THI PHUNG",
  "phone": "0912345678",
  "room": "B22",
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
    "cardNumber": "2848255313",
    "type": 1,
    "vehiclePlate": "94L1-05580",
    "skipValidateVehiclePlate": false,
    "info": "DUONG THI PHUNG",
    "phone": "0912345678",
    "room": "B22",
    "userId": null,
    "status": 1,
    "createdAt": "2026-10-22T10:20:00.0000000Z",
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
    "message": "Card with CardNumber '2848255313' already exists.",
    "details": null
  }
}
```

---

### 4.5. `GET /api/cards` — Danh Sách Thẻ (Phân Trang & Lọc)

#### Request:
```http
GET /api/cards?pageNumber=1&pageSize=10&room=B22
```

#### Response (HTTP 200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "a1d7f8e2-4567-4a12-9c34-88aa11bb22cc",
        "cardNumber": "2848255313",
        "type": 1,
        "vehiclePlate": "94L1-05580",
        "skipValidateVehiclePlate": false,
        "info": "DUONG THI PHUNG",
        "phone": "0912345678",
        "room": "B22",
        "userId": null,
        "status": 1,
        "createdAt": "2026-10-22T10:20:00.0000000Z",
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
    "cardNumber": "2848255313",
    "type": 1,
    "vehiclePlate": "94L1-05580",
    "skipValidateVehiclePlate": false,
    "info": "DUONG THI PHUNG",
    "phone": "0912345678",
    "room": "B22",
    "userId": null,
    "status": 1,
    "createdAt": "2026-10-22T10:20:00.0000000Z",
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
