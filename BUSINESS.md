# 📋 Core Parking Service - Business & Implementation Documentation

> **Dự án**: Core Parking Service (Hệ thống quản lý bãi đỗ xe thông minh)  
> **Nền tảng**: C# / .NET 8, ASP.NET Core Web API, SQLite with EF Core 8  
> **Cập nhật lần cuối**: 2026-10-24  
> **Trạng thái**: ✅ Hoàn thành thiết kế chuẩn hóa 2 bảng sạch (Users & Cards), liên kết 1 User - 1 Thẻ, tích hợp cờ `SkipValidatePlate` và số xe phục vụ đối chiếu ra/vào.

---

## 1. 🎯 Thiết Kế Kiến Trúc 2 Bảng Chuẩn Hóa (Không Dư Thừa)

Hệ thống được thiết kế tách biệt rõ ràng giữa **Thông tin cá nhân/phương tiện của Người dùng** và **Thông tin Thẻ RFID**:

```
 ┌─────────────────────────────────────────┐            ┌─────────────────────────────────────────┐
 │               Users                     │            │                 Cards                   │
 ├─────────────────────────────────────────┤            ├─────────────────────────────────────────┤
 │ Id (PK, Guid)                           │ 1        * │ Id (PK, Guid)                           │
 │ FullName (string)                       │────────────│ CardNumber (string, Unique)             │
 │ Phone (string?)                         │   UserId   │ UserId (FK -> Users.Id, Nullable)       │
 │ Email (string?)                         │ (1-1 Rule) │ Type (Registered, Temporary, Unknown)   │
 │ PlateNumber (string?, Indexed)          │            │ Status (Active = "active", Inactive)    │
 │ Room (string?, Số phòng/căn hộ)         │            │ SkipValidatePlate (bool, default false) │
 │ DateOfBirth, Gender                     │            │ CreatedAt (DateTime UTC)                │
 │ Status (Active = 1, Disabled = 2)       │            │ UpdatedAt (DateTime? UTC)               │
 │ CreatedAt, UpdatedAt, IsDeleted         │            │ IsDeleted (bool, Soft Delete)           │
 └─────────────────────────────────────────┘            └─────────────────────────────────────────┘
```

### 1.1. Bảng `Users` (Thông tin cá nhân & Số xe)
- **Mục đích**: Lưu trữ thông tin định danh của người dùng/cư dân/chủ xe.
- **Các trường**:
  - `Id`: Khóa chính Guid
  - `FullName`: Họ và tên (bắt buộc)
  - `Phone`: Số điện thoại liên hệ
  - `Email`: Địa chỉ email
  - `PlateNumber`: **Số xe / Biển số xe đăng ký** (được đánh Index phục vụ tìm kiếm & đối chiếu cổng ra/vào)
  - `Room`: Số phòng / căn hộ cư dân (ví dụ: `"A16"`, `"B22"`)
  - `DateOfBirth`, `Gender`: Ngày sinh, giới tính
  - `Status`: Trạng thái (`Active` = 1, `Disabled` = 2)
  - `CreatedAt`, `UpdatedAt`, `IsDeleted`: Thời gian UTC & cờ soft-delete

### 1.2. Bảng `Cards` (Thông tin thẻ RFID)
- **Mục đích**: Quản lý danh sách thẻ chip RFID/Mifare, trạng thái thẻ, liên kết người dùng và cờ cấu hình đối soát biển số.
- **Các trường**:
  - `Id`: Khóa chính Guid
  - `CardNumber`: Mã số in hoặc đọc từ thẻ (Unique Index)
  - `UserId`: Khóa ngoại trỏ tới `Users.Id` (Nullable để hỗ trợ thẻ chưa gán hoặc thẻ khách vãng lai)
  - `Type`: Phân loại thẻ (`Registered` = 1, `Temporary` = 2, `Unknown` = 3)
  - `Status`: Trạng thái thẻ (`Active` = "active", `Inactive` = "inactive")
  - `SkipValidatePlate`: **Cờ bỏ qua đối chiếu biển số xe** (bool, mặc định `false`). Nếu `true`, hệ thống cho phép xe qua barrier mà không bắt buộc trùng khớp với biển số xe đăng ký của người dùng (tương thích trường `skipValidateVehiclePlate` từ hệ thống cũ).
  - `CreatedAt`, `UpdatedAt`, `IsDeleted`: Thời gian UTC & cờ soft-delete
- **Loại bỏ hoàn toàn các trường dư thừa**: Các trường như tên, số điện thoại, biển số xe, số phòng không lưu trực tiếp ở bảng `Cards` mà lấy từ `Users` qua quan hệ `UserId`.

---

## 2. 🚦 Quy Trình Kiểm Thử & Đối Chiếu Ra/Vào (Validation Logic)

Khi phương tiện tới barrier quẹt thẻ:
1. **Đầu vào**:
   - `CardNumber`: Mã thẻ đọc từ đầu đọc thẻ USB
   - `DetectedPlateNumber`: Biển số xe do OCR Service nhận diện từ camera RTSP
2. **Luồng kiểm tra của Core Service**:
   ```
   Quẹt thẻ (CardNumber) ──▶ Tìm Card trong DB
                               │
                               ├── 1. Card == null hoặc IsDeleted? ────────────▶ TỪ CHỐI (DENIED)
                               ├── 2. Card.Status != Active? (Bị khóa/mất) ────▶ TỪ CHỐI (DENIED)
                               ├── 3. Card.UserId == null? (Thẻ chưa gán) ────▶ Xử lý thẻ vãng lai
                               │
                               ▼ Thẻ cư dân (Registered)
                       Lấy User từ Card.UserId
                               │
                               ├── 4. User.Status != Active? (Vô hiệu hóa) ────▶ TỪ CHỐI (DENIED)
                               ├── 5. Card.SkipValidatePlate == true? ────────▶ BỎ QUA KIỂM TRA BIỂN SỐ ──▶ CHẤP THUẬN (ALLOWED)
                               ├── 6. User.PlateNumber != DetectedPlate? ─────▶ TỪ CHỐI (DENIED)
                               │
                               ▼
                       CHẤP THUẬN (ALLOWED) ──▶ Gửi lệnh mở barrier
   ```

---

## 3. 📡 Danh Sách API Chính

### 3.1. Quản Lý Thẻ (Cards)
- `POST /api/cards`: Thêm thẻ mới vào danh sách (hỗ trợ `userId`, `type`, `status`, `skipValidatePlate`).
- `GET /api/cards`: Lấy danh sách thẻ (hỗ trợ phân trang, lọc theo `status`, `type`, `userId`, tìm kiếm `searchTerm`). Kết quả trả về kèm `skipValidatePlate` và thông tin người dùng sở hữu (`userFullName`, `userPlateNumber`, `userRoom`, `userStatus`).
- `GET /api/cards/{id}`: Lấy chi tiết thẻ kèm cờ `skipValidatePlate` và biển số xe người dùng.
- `PUT /api/cards/{id}/assign-user`: **Liên kết thẻ với 1 User (quy tắc 1 User - 1 Thẻ)** hoặc hủy liên kết (`userId = null`).
- `PUT /api/cards/{id}/status`: Cập nhật trạng thái thẻ (`active` / `inactive`).
- `PUT /api/cards/{id}/skip-validate-plate`: Cập nhật cấu hình cờ `skipValidatePlate` (`true` / `false`) cho thẻ.

### 3.2. Quản Lý Người Dùng (Users)
- `POST /api/users`: Thêm người dùng mới (có sẵn `plateNumber`, `room`, `phone`, và tùy chọn gán ngay `cardId`).
- `GET /api/users`: Lấy danh sách người dùng (hỗ trợ phân trang, lọc theo `status`, `room`, tìm kiếm theo tên, biển số, phòng, SĐT).
- `GET /api/users/{id}`: Lấy chi tiết người dùng kèm mã thẻ đang liên kết (`linkedCardNumber`).

---

## 4. 📊 Dữ Liệu Nạp Thực Tế (Từ `data.sql`)

Dữ liệu từ `data.sql` đã được import và chuẩn hóa sạch sẽ:
- **Tổng số Users**: **91** (3 tài khoản hệ thống + 88 cư dân kèm số xe và số phòng).
- **Tổng số Cards**: **529** (88 thẻ đã được liên kết 1-1 với User có số xe + 441 thẻ chưa gán / thẻ vãng lai).
- **Thẻ cấu hình SkipValidatePlate = true**: **2 thẻ** được nhận diện từ `data.sql`:
  - Mã thẻ `0108394918` (ID: `c0000000-0000-0000-0000-000000000162`)
  - Mã thẻ `0354547274` (ID: `c0000000-0000-0000-0000-000000000338`, liên kết cư dân xe `51F-123.45`)
- **Tập tin cơ sở dữ liệu SQLite**: `src/ParkingService.Api/parking_service.db`.
- **Script chuẩn hóa dữ liệu**: `python scripts/import_data.py`.
