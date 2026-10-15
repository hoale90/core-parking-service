# Quy Tắc Nghiệp Vụ Quản Lý Bãi Xe (Parking Domain Rules)

> Nguồn chính: [parking_management_requirements.md](file:///D:/Projects/parking-manager/parking-service/core-parking-service/parking_management_requirements.md)

---

## 1. Phạm Vi Giai Đoạn 1 (Phase 1 Scope)
- Chỉ xây dựng các thành phần sau:
  - **Entity `User`**:
    - Các trường: `Id`, `FullName`, `DateOfBirth`, `Gender`, `Phone`, `Email`, `Status` (Active/Disabled), `CreatedAt`, `UpdatedAt`.
  - **Entity `Card`**:
    - Các trường: `Id`, `CardNumber` (unique), `UserId` (nullable), `Status` (Active/Blocked/Lost), `CreatedAt`, `UpdatedAt`.
    - `UserId = null` cho phép thẻ chưa gán người dùng hoặc thẻ dành cho khách.
    - Nếu `UserId` có giá trị thì người dùng đó phải tồn tại trong hệ thống.
  - **APIs**:
    - `POST /api/users`
    - `GET /api/users`
    - `GET /api/users/{id}`
    - `POST /api/cards`
    - `GET /api/cards`
    - `GET /api/cards/{id}`

---

## 2. Các Nguyên Tắc Bất Di Bất Dịch (Section 8)

1. **Quyết định cho phép ra/vào**: Core Service là nơi duy nhất quyết định `Allowed` hoặc `Denied`. OCR Service và Desktop App không có thẩm quyền quyết định.
2. **Thẻ chưa gán người dùng (`UserId = null`)**: KHÔNG mặc nhiên được tự động mở barrier.
3. **Lệnh Barrier**: Việc gửi lệnh mở barrier thành công không chứng minh barrier đã mở thực tế.
4. **Bảo mật**: Tuyệt đối không lưu mật khẩu hoặc khóa kết nối dạng plain text.
5. **Định dạng dữ liệu**:
   - Khóa chính: UUID (`TEXT` trong SQLite, `Guid` trong C#).
   - Thời gian: ISO 8601 UTC.
   - Trạng thái xe, thẻ, vị trí: Dùng `enum` rõ ràng, không dùng magic strings/numbers.

---

## 3. Các Mục Chưa Xác Nhận (Không Tự Ý Đoán Nghiệp Vụ)
- Nghiệp vụ khách vãng lai (khách không có `UserId` có được tự động vào hay cần nhân viên duyệt).
- Quy tắc gán một thẻ cho nhiều xe hay 1 xe.
- Model thiết bị USB, relay và giao thức phần cứng.
- Cơ chế cấp phép phần mềm (license).
- **Mọi chi tiết thuộc các mục này phải chờ User chỉ định cụ thể trước khi code.**
