# Parking Management System — Requirements đã xác nhận

> Tài liệu tổng hợp các yêu cầu đã xác nhận trong cuộc trao đổi. Những phần chưa chốt được ghi rõ là **Chưa xác nhận** để tránh hiểu nhầm là yêu cầu cuối cùng.

## 1. Tổng quan dự án

- Hệ thống quản lý bãi giữ xe, chủ yếu là xe máy tại Việt Nam.
- Phiên bản đầu chạy trên một máy Windows; kiến trúc nên có khả năng mở rộng sang máy chủ riêng sau này.
- Giao diện desktop: C# / .NET với WPF.
- Core Service: C# / ASP.NET Core Web API.
- Database: SQLite.
- OCR Service: Python độc lập, giao tiếp qua HTTP API.
- Camera: RTSP.
- Đầu đọc thẻ và bộ điều khiển barrier: thiết bị USB.
- Có hai làn: làn vào (`IN`) và làn ra (`OUT`).

## 2. Kiến trúc và nguyên tắc

- Core Service là nơi quyết định cho phép hoặc từ chối xe ra vào.
- OCR Service chỉ nhận ảnh và trả kết quả nhận diện biển số; không quyết định quyền ra vào.
- Desktop App không tự quyết định quyền truy cập và không gọi relay trực tiếp.
- Core Service quản lý nghiệp vụ, truy cập database và tích hợp OCR/thiết bị.
- Giao tiếp nội bộ ban đầu qua HTTP trên máy cục bộ.
- Sự kiện ra vào cần có mã định danh duy nhất (`EventId`) để hạn chế ghi trùng khi request được gửi lại.
- Quyết định `Allowed` không đồng nghĩa barrier chắc chắn đã mở.

## 3. Database đã thống nhất

### 3.1. Quy tắc chung

- Sử dụng SQLite, tương thích C#/.NET và Entity Framework Core.
- Khóa chính UUID lưu dưới dạng `TEXT`.
- Thời gian lưu dạng ISO 8601, thống nhất dùng UTC.
- Bật kiểm tra khóa ngoại.
- Dùng `NOT NULL`, `UNIQUE`, `CHECK`, khóa ngoại và index phù hợp.
- Ưu tiên vô hiệu hóa thay vì xóa vật lý dữ liệu đã có lịch sử.
- Có migration, sao lưu và khôi phục database.
- Ảnh chụp lưu dưới dạng file; database lưu đường dẫn ảnh.

### 3.2. `Users`

Các trường:
- `Id`
- `FullName`
- `DateOfBirth` (nullable)
- `Gender` (nullable)
- `Phone` (nullable)
- `Email` (nullable)
- `Status`: `Active` hoặc `Disabled`
- `CreatedAt`
- `UpdatedAt`

### 3.3. `Cards`

Các trường:
- `Id`
- `CardNumber` (bắt buộc, duy nhất)
- `UserId` (nullable, khóa ngoại tới `Users`)
- `Status`: `Active`, `Blocked` hoặc `Lost`
- `CreatedAt`
- `UpdatedAt`

Quy tắc:
- Không có `ValidFrom` và `ValidTo`.
- `UserId = NULL` cho phép lưu thẻ chưa gán người dùng hoặc thẻ dành cho khách.
- Thẻ chưa gán người dùng **không mặc nhiên được phép vào bãi**.
- Không xóa vật lý thẻ đã có lịch sử giao dịch.

### 3.4. `Vehicles`

Các trường:
- `Id`
- `UserId`
- `PlateNumber`
- `PlateNormalized`
- `VehicleType` (mặc định `Motorcycle`)
- `Brand` (nullable)
- `Color` (nullable)
- `Status`: `Active` hoặc `Disabled`
- `CreatedAt`
- `UpdatedAt`

Quy tắc:
- Tạo index cho `UserId` và `PlateNormalized`.
- `PlateNormalized` dùng để so sánh biển số đăng ký với kết quả OCR.
- Chưa đặt unique cho `PlateNormalized` vì quy tắc biển số trùng chưa được chốt.

### 3.5. `Lanes`

Các trường:
- `Id`
- `LaneCode` (duy nhất)
- `LaneName`
- `Direction`: `IN` hoặc `OUT`
- `IsEnabled`
- `CreatedAt`

### 3.6. `Devices`

Các trường:
- `Id`
- `DeviceCode` (duy nhất)
- `DeviceType`: `Camera`, `CardReader` hoặc `BarrierRelay`
- `LaneId` (nullable)
- `ConnectionType`
- `ConfigurationJson` (nullable)
- `IsEnabled`
- `CreatedAt`
- `UpdatedAt`

Không lưu bí mật thiết bị dạng văn bản thuần trong `ConfigurationJson`.

### 3.7. `AccessEvents`

Các trường:
- `Id`
- `EventId` (duy nhất, dùng chống ghi trùng)
- `CardId` (nullable)
- `LaneId`
- `Direction`: `IN` hoặc `OUT`
- `EventTime`
- `OcrPlateNumber` (nullable)
- `PlateNormalized` (nullable)
- `OcrConfidence` (nullable)
- `VehicleId` (nullable)
- `Decision`: `Allowed`, `Denied` hoặc `ManualReview`
- `ReasonCode` (nullable)
- `SnapshotPath` (nullable)

Không đưa các trường sau vào `AccessEvents` ở phiên bản đầu:
- `SnapshotSha256`
- `BarrierCommandStatus`
- `ProcessingMs`

Trạng thái lệnh barrier và lỗi thiết bị có thể được lưu ở bảng riêng.

### 3.8. `ParkingSessions`

Các trường:
- `Id`
- `VehicleId`
- `EntryEventId` (duy nhất)
- `ExitEventId` (nullable, duy nhất khi có giá trị)
- `EntryTime`
- `ExitTime` (nullable)
- `Status`: `Inside`, `Exited` hoặc `Exception`
- `CreatedAt`
- `UpdatedAt`

Quy tắc:
- Tạo phiên gửi xe khi lượt vào được xác nhận hợp lệ.
- Kết thúc phiên khi lượt ra được xác nhận hợp lệ.
- Mỗi phương tiện chỉ có tối đa một phiên ở trạng thái `Inside`.
- Cập nhật sự kiện và phiên gửi xe bằng transaction khi cần để giữ tính nhất quán.

### 3.9. Tài khoản và phân quyền

`AppUsers`:
- `Id`, `Username` (duy nhất), `PasswordHash`, `DisplayName`, `IsEnabled`, `FailedLoginCount`, `LockedUntil` (nullable), `CreatedAt`, `UpdatedAt`.

`Roles`:
- `Id`, `RoleCode` (duy nhất), `RoleName`.

`AppUserRoles`:
- `AppUserId`, `RoleId`, khóa chính kết hợp `(AppUserId, RoleId)`.

Không lưu mật khẩu dạng văn bản thuần.

### 3.10. `AuditLogs`

Các trường:
- `Id`
- `ActorUserId` (nullable)
- `Action`
- `EntityType`
- `EntityId` (nullable)
- `OccurredAt`
- `Reason` (nullable)
- `BeforeJson` (nullable)
- `AfterJson` (nullable)
- `CorrelationId` (nullable)

### 3.11. Bảng hỗ trợ được đề xuất

Các bảng được đề xuất để xem xét khi triển khai:
- `DeviceEvents`: lịch sử trạng thái/lỗi thiết bị.
- `AppSettings`: cấu hình ứng dụng.
- `LicenseState`: trạng thái giấy phép.
- `SchemaMigrations` hoặc dùng EF Core Migrations, không cần duy trì hai cơ chế trùng chức năng.
- `ImportJobs` và tùy chọn `ImportJobRows`: theo dõi nhập dữ liệu.
- `LicenseActivations`: nếu cần theo dõi kích hoạt giấy phép.
- `BarrierCommands`: lịch sử lệnh barrier và trạng thái xác nhận.

## 4. OCR Service đã xác nhận

### 4.1. Công nghệ

- Python.
- FastAPI.
- OpenCV.
- PaddleOCR chạy CPU, không yêu cầu GPU.
- Có thể đóng gói thành `.exe` cho Windows.
- OCR Service không truy cập database và không điều khiển barrier.

### 4.2. API

`GET /health`
- Kiểm tra trạng thái service và OCR engine.

`POST /ocr`
- Nhận ảnh trực tiếp qua `multipart/form-data`.
- Trường upload tên `image`.
- Không yêu cầu Base64.

### 4.3. Response thành công

Ví dụ:

```json
{
  "success": true,
  "data": {
    "plate_number": "59X112345",
    "plate_display": "59-X1 123.45",
    "confidence": 0.95,
    "processing_ms": 350
  },
  "error": null
}
```

Các giá trị chỉ là ví dụ.

### 4.4. Response không tìm thấy biển số

Không tìm thấy biển số là kết quả xử lý hợp lệ, không phải lỗi API:

```json
{
  "success": true,
  "data": {
    "plate_number": null,
    "plate_display": null,
    "confidence": null,
    "processing_ms": 350
  },
  "error": null
}
```

### 4.5. Response lỗi

Ví dụ lỗi ảnh không hợp lệ — HTTP 400:

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "INVALID_IMAGE",
    "message": "The uploaded file is not a valid image."
  }
}
```

Các mã lỗi đã đề xuất:
- HTTP 400: `INVALID_IMAGE`
- HTTP 413: `FILE_TOO_LARGE`
- HTTP 415: `UNSUPPORTED_MEDIA_TYPE`
- HTTP 503: `OCR_NOT_READY`
- HTTP 500: `OCR_PROCESSING_ERROR`

Quy tắc:
- Response luôn có `success`, `data`, `error`.
- Khi thành công: `error = null`.
- Khi lỗi API: `data = null`, `error` có `code` và `message`.
- Không tìm thấy biển số trả HTTP 200 với các trường biển số bằng `null`.
- `confidence` nằm trong khoảng 0–1 hoặc `null`.
- Không trả stack trace hoặc thông tin nội bộ cho client.

## 5. Core Service — phạm vi API đã yêu cầu ở giai đoạn đầu

### 5.1. Công nghệ

- C# / ASP.NET Core Web API.
- SQLite.
- Entity Framework Core.
- Tự tạo/cập nhật database bằng EF Core Migrations.
- Swagger để kiểm thử API.
- DTO riêng cho request/response, không trả trực tiếp EF Core entity.
- Thời gian lưu UTC.
- Validate dữ liệu đầu vào và trả HTTP status code phù hợp.

### 5.2. Entity `User`

Các trường:
- `Id`
- `FullName`
- `DateOfBirth`
- `Gender`
- `Phone`
- `Email`
- `Status`
- `CreatedAt`
- `UpdatedAt`

### 5.3. Entity `Card`

Các trường:
- `Id`
- `CardNumber`
- `UserId` (nullable)
- `Status`
- `CreatedAt`
- `UpdatedAt`

Quy tắc:
- `CardNumber` phải duy nhất.
- `UserId` có thể `NULL`.
- Nếu `UserId` có giá trị thì người dùng đó phải tồn tại.

### 5.4. API đã yêu cầu

- `POST /api/users` — thêm người dùng.
- `GET /api/users` — lấy danh sách người dùng.
- `GET /api/users/{id}` — lấy chi tiết người dùng.
- `POST /api/cards` — thêm thẻ, cho phép `userId = null`.
- `GET /api/cards` — lấy danh sách thẻ.
- `GET /api/cards/{id}` — lấy chi tiết thẻ.

Phạm vi ban đầu chỉ gồm User và Card; không tự thêm nghiệp vụ khác vào phiên bản đầu.

## 6. Yêu cầu triển khai và đóng gói

- Ứng dụng dự kiến chạy trên Windows.
- OCR Service cần có hướng dẫn đóng gói độc lập thành `.exe`.
- Cần có hướng dẫn cài đặt, chạy và kiểm thử API.
- Cần có phương án sao lưu/khôi phục SQLite và các ảnh liên quan.
- Giấy phép phần mềm: đã nêu nhu cầu hỗ trợ cả kích hoạt offline và online cho sản phẩm thương mại; chi tiết triển khai chưa được chốt.

## 7. Các điểm chưa xác nhận

Các mục sau **chưa được chốt thành yêu cầu cuối cùng**:

1. **Nghiệp vụ khách vãng lai:** thẻ chưa gán `UserId` có được tạo lượt vào tự động hay cần nhân viên nhập biển số/duyệt trước.
2. **Liên kết thẻ và xe:** quy tắc gán một thẻ cho một hay nhiều phương tiện, đặc biệt với khách vãng lai.
3. **Đầu đọc thẻ USB:** model và giao thức cụ thể chưa được chọn.
4. **USB Relay/IO:** model, giao thức, khả năng xác nhận barrier đã mở và tích hợp cảm biến chưa được xác định.
5. **OCR:** độ chính xác thực tế, ROI camera, bộ ảnh benchmark và ngưỡng confidence cần được kiểm chứng.
6. **Thiết bị và dịch vụ Windows:** thành phần nào chạy Windows Service hay trong user session cần quyết định theo driver và SDK thiết bị.
7. **Cấp phép thương mại:** chính sách hết hạn, gia hạn, số máy, chuyển máy và thời gian hiệu lực license chưa được chốt.
8. **Bảng hỗ trợ database:** cần xác định bảng nào thực sự được đưa vào migration đầu tiên.

---

## 8. Nguyên tắc không tự suy diễn

Khi triển khai các yêu cầu này:
- Không tự xem các đề xuất là quyết định nghiệp vụ cuối cùng nếu được đánh dấu chưa xác nhận.
- Không để OCR tự quyết định quyền vào/ra.
- Không cho phép thẻ chưa gán người dùng tự động được cấp quyền chỉ vì `UserId` là `NULL`.
- Không coi gửi lệnh mở barrier thành công là bằng chứng barrier đã mở.
- Không ghi mật khẩu hoặc bí mật dạng văn bản thuần.
- Giữ API, schema và tên trường nhất quán giữa các service.
