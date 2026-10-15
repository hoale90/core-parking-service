# 📋 Parking Manager - Business Requirements

> **Project**: Parking Management System  
> **Service**: Core Parking Service  
> **Last Updated**: 2026-10-15  
> **Status**: 🚧 In Progress

---

## 1. Tổng Quan Hệ Thống (System Overview)

Hệ thống quản lý bãi đỗ xe (**Parking Manager**) giúp quản lý và vận hành bãi đỗ xe một cách hiệu quả, từ việc quản lý vị trí đỗ xe, ghi nhận xe ra/vào, tính phí, đến báo cáo thống kê.

### Mục tiêu chính:
- [ ] Quản lý bãi đỗ xe (Parking Lot Management)
- [ ] Quản lý vị trí đỗ xe (Parking Spot Management)
- [ ] Ghi nhận xe ra/vào (Vehicle Check-in/Check-out)
- [ ] Tính phí đỗ xe (Pricing & Billing)
- [ ] Báo cáo & thống kê (Reporting & Analytics)

---

## 2. Modules & Features

### 2.1 🏢 Quản Lý Bãi Đỗ Xe (Parking Lot)
| Feature | Mô tả | Status |
|---------|--------|--------|
| Tạo bãi đỗ xe | Tạo mới bãi đỗ xe với thông tin: tên, địa chỉ, sức chứa | ⬜ Todo |
| Cập nhật thông tin | Sửa thông tin bãi đỗ xe | ⬜ Todo |
| Xóa bãi đỗ xe | Soft delete bãi đỗ xe | ⬜ Todo |
| Xem danh sách | Lấy danh sách bãi đỗ xe (có phân trang) | ⬜ Todo |
| Xem chi tiết | Xem chi tiết một bãi đỗ xe | ⬜ Todo |

### 2.2 🅿️ Quản Lý Vị Trí Đỗ Xe (Parking Spot)
| Feature | Mô tả | Status |
|---------|--------|--------|
| Tạo vị trí | Tạo vị trí đỗ xe trong bãi (tên, tầng, loại xe) | ⬜ Todo |
| Cập nhật trạng thái | Available / Occupied / Reserved / Maintenance | ⬜ Todo |
| Xem vị trí trống | Lọc danh sách vị trí theo trạng thái | ⬜ Todo |

### 2.3 🚗 Ghi Nhận Xe Ra/Vào (Parking Session)
| Feature | Mô tả | Status |
|---------|--------|--------|
| Check-in | Ghi nhận xe vào bãi (biển số, loại xe, thời gian vào) | ⬜ Todo |
| Check-out | Ghi nhận xe rời bãi (thời gian ra, tính phí) | ⬜ Todo |
| Xem lịch sử | Xem lịch sử đỗ xe theo biển số / thời gian | ⬜ Todo |

### 2.4 💰 Tính Phí (Pricing)
| Feature | Mô tả | Status |
|---------|--------|--------|
| Cấu hình giá | Đặt giá theo loại xe, theo giờ/ngày | ⬜ Todo |
| Tính phí tự động | Tự động tính phí khi check-out | ⬜ Todo |

### 2.5 📊 Báo Cáo (Reporting)
| Feature | Mô tả | Status |
|---------|--------|--------|
| Thống kê doanh thu | Doanh thu theo ngày/tuần/tháng | ⬜ Todo |
| Tỷ lệ sử dụng | Tỷ lệ chiếm dụng bãi đỗ xe | ⬜ Todo |

---

## 3. API Response Chuẩn Hóa

Tất cả API response đều theo format:

### Success
```json
{
  "success": true,
  "data": { ... },
  "error": null
}
```

### Error
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred.",
    "details": {
      "licensePlate": ["License plate is required."]
    }
  }
}
```

> **Note**: `data` và `error` có thể là object hoặc `null`.

---

## 4. Loại Xe Hỗ Trợ (Vehicle Types)

| Enum Value | Loại xe |
|------------|---------|
| 0 | Car (Ô tô) |
| 1 | Motorcycle (Xe máy) |
| 2 | Truck (Xe tải) |
| 3 | Bicycle (Xe đạp) |

## 5. Trạng Thái Vị Trí Đỗ Xe (Parking Spot Status)

| Enum Value | Trạng thái |
|------------|-----------|
| 0 | Available (Trống) |
| 1 | Occupied (Đang sử dụng) |
| 2 | Reserved (Đã đặt trước) |
| 3 | Maintenance (Bảo trì) |

---

## 6. Ghi Chú Kỹ Thuật

- **Architecture**: Clean Architecture (Domain → Application → Infrastructure → API)
- **Database**: SQL Server with Entity Framework Core
- **Pattern**: CQRS with MediatR, Repository + Unit of Work
- **Validation**: FluentValidation with MediatR Pipeline Behavior
- **Logging**: Serilog (Console + File)
- **API Docs**: Swagger / OpenAPI
- **Soft Delete**: Tất cả entity đều hỗ trợ soft delete (`IsDeleted`)

---

## 7. Backlog / Yêu Cầu Tiếp Theo

> _Ghi chú các yêu cầu mới từ user tại đây._

| # | Yêu cầu | Ngày nhận | Status |
|---|---------|-----------|--------|
| | | | |

---

*📌 Document này sẽ được cập nhật liên tục khi có yêu cầu mới.*
