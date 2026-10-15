# Tiêu Chuẩn Viết Test Thực Tế, Không Ảo (Realistic Testing Standards)

> **Mục tiêu**: Đảm bảo bộ test phản ánh chính xác hành vi hệ thống trong thực tế. Tuyệt đối không chấp nhận các test case mang tính hình thức, che giấu lỗi hoặc chạy qua loa chỉ để tăng code coverage ảo.

---

## 1. 🚫 Những Loại Test Ảo BỊ CẤM

1. **Assert rỗng hoặc vô nghĩa**:
   ```csharp
   // ❌ CẤM: Tautological assertion
   Assert.True(true);
   result.Should().NotBeNull(); // Không kiểm tra nội dung bên trong

   // ✅ ĐÚNG: Assert cụ thể trạng thái và dữ liệu thực tế
   result.Success.Should().BeTrue();
   result.Data.Should().NotBeNull();
   result.Data!.CardNumber.Should().Be("0012345678");
   result.Data.Status.Should().Be(CardStatus.Active);
   ```

2. **Mock chiếu lệ không có nghiệp vụ**:
   ```csharp
   // ❌ CẤM: Mock một phương thức trả về null rồi assert kết quả là null mà không test logic
   _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((User)null);
   // ... không assert Exception, không test branch

   // ✅ ĐÚNG: Test kịch bản thực tế khi Entity không tồn tại
   var nonExistentId = Guid.NewGuid();
   _mockRepo.Setup(r => r.GetByIdAsync(nonExistentId, default)).ReturnsAsync((User?)null);
   
   var act = async () => await _handler.Handle(new GetUserByIdQuery(nonExistentId), default);
   await act.Should().ThrowAsync<NotFoundException>()
       .WithMessage($"*User*{nonExistentId}*");
   ```

3. **Bỏ qua Negative Cases và Edge Cases**:
   - Không được chỉ viết Happy Path.
   - Bắt buộc phải viết test cho: dữ liệu trùng lặp (Duplicate Key), định dạng sai, trường bắt buộc bị null/rỗng, giá trị biên.

---

## 2. 🎯 Yêu Cầu Cụ Thể Từng Loại Test

### 2.1. FluentValidation Tests
- Phải test cụ thể từng rule:
  - Khi trường rỗng (`""`, `null`, whitespace).
  - Khi độ dài vượt quá giới hạn (ví dụ: `CardNumber` quá 50 ký tự).
  - Khi sai định dạng regex (email, số điện thoại, biển số xe).
- Dùng `TestValidate` của FluentValidation:
  ```csharp
  var result = _validator.TestValidate(command);
  result.ShouldHaveValidationErrorFor(x => x.CardNumber)
        .WithErrorMessage("Card number is required.");
  ```

### 2.2. MediatR Handler Tests
- Test với dữ liệu thực tế:
  - Happy path: lưu thành công, gọi `AddAsync` và `SaveChangesAsync`.
  - Conflict path: ném `DomainException` hoặc `ValidationException` khi trùng lặp `CardNumber`.
  - Not found path: ném `NotFoundException` khi tìm kiếm ID không tồn tại.
  - Foreign key path: kiểm tra `UserId` có tồn tại không trước khi gán thẻ.

### 2.3. EF Core / Database Integration Tests
- Sử dụng SQLite In-Memory:
  ```csharp
  var connection = new SqliteConnection("DataSource=:memory:");
  connection.Open();
  var options = new DbContextOptionsBuilder<ApplicationDbContext>()
      .UseSqlite(connection)
      .Options;
  ```
- Kiểm tra tính toàn vẹn:
  - Ràng buộc Unique Index (ví dụ: không cho phép 2 `CardNumber` giống nhau).
  - Ràng buộc Foreign Key (gán `UserId` không có trong bảng `Users` phải ném exception).
  - Soft Delete Query Filter: entity có `IsDeleted = true` không xuất hiện trong `GetAllAsync()` hoặc query thông thường.

---

## 3. 📋 Dữ Liệu Test Thực Tế (Realistic Fixtures)

- **Biển số xe Việt Nam**: `"59-X1 123.45"`, `"29A-999.99"`, `"43B-1234"`
- **Số thẻ**: Mã thẻ Mifare / RFID thực tế `"0841295731"`, `"0019283746"`
- **Số điện thoại**: `"0912345678"`, `"0987654321"`
- **Email**: `"nguyen.van.a@example.com"`
- **Thời gian**: Thống nhất dùng UTC (`DateTime.UtcNow`)
