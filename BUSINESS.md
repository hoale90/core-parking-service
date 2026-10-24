# 📋 Core Parking Service - Business & Implementation Documentation

> **Dự án**: Core Parking Service (Hệ thống quản lý bãi đỗ xe thông minh)  
> **Nền tảng**: C# / .NET 8, ASP.NET Core Web API, SQLite with EF Core 8  
> **Cập nhật lần cuối**: 2026-10-24  
> **Trạng thái**: ✅ Hoàn thành thiết kế chuẩn hóa 4 bảng (Users, Cards, AccessEvents, ParkingSessions), liên kết 1 User - 1 Thẻ, tích hợp cờ `SkipValidatePlate`, thuật toán so khớp biển số xe Levenshtein (%) và API quẹt thẻ ra/vào.

---

## 1. 🎯 Thiết Kế Cơ Sở Dữ Liệu Chuẩn Hóa

Hệ thống được thiết kế theo kiến trúc Clean Architecture, tách biệt rõ giữa **Master Data** (Người dùng, Thẻ) và **Transaction / Audit Log** (Sự kiện quẹt thẻ, Phiên gửi xe):

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
 │ Status (Active = 1, Disabled = 2)       │            │ CreatedAt, UpdatedAt, IsDeleted         │
 └─────────────────────────────────────────┘            └─────────────────────────────────────────┘
                      ▲                                              ▲
                      │                                              │
                      └──────────────────────┐                       │
                                             │                       │
 ┌─────────────────────────────────────────┐ │          ┌────────────┴────────────────────────────┐
 │            AccessEvents                 │ │          │             ParkingSessions             │
 ├─────────────────────────────────────────┤ │          ├─────────────────────────────────────────┤
 │ Id (PK, Guid)                           │ │          │ Id (PK, Guid)                           │
 │ CardNumber (string, Indexed)            │ │          │ CardNumber (string, Indexed)            │
 │ CardId (FK -> Cards.Id, Nullable)───────┼─┘          │ CardId (FK -> Cards.Id, Nullable)       │
 │ UserId (FK -> Users.Id, Nullable)───────┘            │ UserId (FK -> Users.Id, Nullable)       │
 │ Direction (In = 1, Out = 2)                          │ PlateNumber (string?, Indexed)          │
 │ LaneCode (string?)                                   │ EntryTime (DateTime UTC)                │
 │ OcrPlateNumber (string?)                             │ ExitTime (DateTime? UTC)                │
 │ PlateNormalized (string?)                            │ DurationMinutes (int?)                  │
 │ ExpectedPlateNumber (string?)                        │ EntryEventId (FK -> AccessEvents.Id)    │
 │ PlateSimilarityPercent (double?)                     │ ExitEventId (FK -> AccessEvents.Id)     │
 │ SkipValidatePlate (bool)                             │ EntrySnapshotPath, ExitSnapshotPath     │
 │ Decision (Allowed = 1, Denied = 2)                   │ Status (Inside = 1, Exited = 2)         │
 │ ReasonCode (string)                                  │ CreatedAt, UpdatedAt, IsDeleted         │
 │ SnapshotPath (string?)                               └─────────────────────────────────────────┘
 │ EventTime (DateTime UTC, Indexed)
 └─────────────────────────────────────────┘
```

---

## 2. 🚦 Quy Trình Quẹt Thẻ & Đối Chiếu Biển Số (Swipe Card Flow)

Khi phương tiện tới cổng kiểm soát quẹt thẻ:
1. **Đầu vào (`POST /api/access/swipe`)**:
   - `CardNumber`: Mã thẻ đọc từ đầu đọc thẻ RFID
   - `Direction`: Hướng `In` (1) hoặc `Out` (2)
   - `Image`: File ảnh chụp xe từ camera tại làn (multipart/form-data)
   - `LaneCode`: Mã làn kiểm soát (ví dụ: `LANE_IN_01`)
   - `CustomThresholdPercent`: Ngưỡng % tùy chỉnh (tùy chọn, mặc định 80.0%)

2. **Luồng kiểm tra của Core Service**:
   ```
   Quẹt thẻ (CardNumber + Image + Direction)
             │
             ├──▶ Lưu ảnh snapshot vào thư mục: snapshots/YYYY/MM/DD/{guid}.jpg
             ├──▶ Gọi OCR Service (Python FastAPI) nhận diện biển số xe từ ảnh
             │
             ▼ Kiểm tra Thẻ trong DB
             ├── 1. Card == null? ─────────────────────────▶ DENIED (CARD_NOT_FOUND)
             ├── 2. Card.Status != Active? (Bị khóa/mất) ──▶ DENIED (CARD_INACTIVE)
             │
             ▼ Thẻ hợp lệ -> Lấy User từ Card.UserId
             ├── 3. User != null && User.Status == Disabled? ──▶ DENIED (USER_DISABLED)
             ├── 4. Card.SkipValidatePlate == true? ──────────▶ ALLOWED (SKIP_VALIDATE_ALLOWED) (Bỏ qua so biển số)
             │
             ▼ Bắt buộc so khớp biển số xe
             ├── 5. User == null hoặc chưa có PlateNumber? ────▶ DENIED (NO_USER_ASSIGNED)
             ├── 6. OCR không nhận diện được biển số? ────────▶ DENIED (OCR_FAILED)
             │
             ▼ Thuật toán so khớp mờ Levenshtein Distance
             ├── Chuẩn hóa: Biển số in hoa, xóa ký tự đặc biệt [^A-Z0-9]
             ├── Tính Similarity% = (1 - Distance / MaxLen) * 100
             ├── 7. Similarity% < Ngưỡng (80%)? ───────────────▶ DENIED (PLATE_MISMATCH)
             │
             ▼
      CHẤP THUẬN (ALLOWED) ──▶ VALID (Mở barrier)
             │
             ├── Direction == In:  Tạo mới ParkingSession (Status = Inside)
             └── Direction == Out: Đóng ParkingSession (Status = Exited, tính DurationMinutes)
             │
             ▼
      Ghi nhận AccessEvents (Audit Log) & Trả về Response
   ```

---

## 3. 📡 Danh Sách API Hoàn Chỉnh

### 3.1. Cổng Kiểm Soát Ra/Vào & Barrier (`AccessController`)
- `POST /api/access/swipe` (multipart/form-data): Quẹt thẻ tại cổng, nhận diện biển số OCR, kiểm tra cờ `SkipValidatePlate`, so khớp biển số, lưu snapshot, ghi log `AccessEvents`, tạo/cập nhật `ParkingSessions` và trả về quyết định `Allowed` / `Denied`.
- `POST /api/access/test-plate-comparison`: API tiện ích kiểm thử và tinh chỉnh thuật toán so khớp giữa 2 biển số bất kỳ với ngưỡng % tùy chọn.
- `GET /api/access/events`: Lấy lịch sử quẹt thẻ (hỗ trợ phân trang, lọc theo `cardNumber`, `direction`, `decision`, `fromDate`, `toDate`).
- `GET /api/access/sessions/inside`: Danh sách xe hiện đang đỗ bên trong bãi (`Status = Inside`).

### 3.2. Quản Lý Thẻ (`CardsController`)
- `POST /api/cards`: Thêm thẻ mới vào kho (hỗ trợ `userId`, `type`, `status`, `skipValidatePlate`).
- `GET /api/cards`: Lấy danh sách thẻ (hỗ trợ phân trang, lọc `active`/`inactive`, `assigned`/`unassigned`, tìm kiếm `searchTerm`).
- `GET /api/cards/{id}`: Lấy chi tiết thẻ kèm biển số xe người dùng và cờ `skipValidatePlate`.
- `PUT /api/cards/{id}/assign-user`: Gán thẻ cho người dùng (quy tắc 1-1) hoặc hủy gán (`userId = null`).
- `PUT /api/cards/{id}/status`: Cập nhật trạng thái thẻ (`active` / `inactive`).
- `PUT /api/cards/{id}/skip-validate-plate`: Cập nhật cờ bỏ qua kiểm tra biển số cho thẻ (`true` / `false`).

### 3.3. Quản Lý Người Dùng (`UsersController`)
- `POST /api/users`: Thêm người dùng mới (kèm `plateNumber`, `room`, `phone`, tùy chọn `cardId`).
- `GET /api/users`: Lấy danh sách người dùng (phân trang, lọc theo `status`, `room`, tìm kiếm).
- `GET /api/users/{id}`: Lấy chi tiết người dùng kèm mã thẻ liên kết (`linkedCardNumber`).

---

## 4. 📊 Dữ Liệu Nạp Thực Tế (Từ `data.sql`)

Dữ liệu từ `data.sql` đã được import và chuẩn hóa sạch sẽ:
- **Tổng số Users**: **91** (3 tài khoản hệ thống + 88 cư dân kèm số xe và số phòng).
- **Tổng số Cards**: **529** (88 thẻ đã được liên kết 1-1 với User có số xe + 441 thẻ chưa gán / thẻ vãng lai).
- **Thẻ cấu hình SkipValidatePlate = true**: **2 thẻ** được nhận diện từ `data.sql`:
  - Mã thẻ `0108394918` (ID: `c0000000-0000-0000-0000-000000000162`)
  - Mã thẻ `0354547274` (ID: `c0000000-0000-0000-0000-000000000338`, liên kết cư dân xe `99E1-22268`)
- **Tập tin cơ sở dữ liệu SQLite**: `src/ParkingService.Api/parking_service.db`.
- **Script chuẩn hóa dữ liệu**: `python scripts/import_data.py`.
