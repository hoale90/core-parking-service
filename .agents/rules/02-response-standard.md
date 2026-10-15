# Quy Chuẩn Định Dạng API Response

## 1. Cấu Trúc Bắt Buộc

Mọi API response từ `ParkingService.Api` phải trả về đúng envelope JSON:

```json
{
  "success": true,
  "data": {},
  "error": null
}
```

- Khi thành công:
  - `success`: `true`
  - `data`: Object dữ liệu (hoặc mảng, hoặc `null` nếu không có dữ liệu trả về)
  - `error`: `null`
- Khi thất bại:
  - `success`: `false`
  - `data`: `null`
  - `error`: Object `{ "code": "...", "message": "...", "details": ... }`

## 2. Các Lớp Hỗ Trợ
- [`ApiResponse<T>`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/src/ParkingService.Application/Common/Models/ApiResponse.cs): Sử dụng cho response có data có kiểu.
- [`ApiResponse`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/src/ParkingService.Application/Common/Models/ApiResponse.cs): Sử dụng cho non-generic response (data = null).
- [`ApiError`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/src/ParkingService.Application/Common/Models/ApiError.cs): Đại diện cho lỗi trả về.

## 3. Controller Guidelines
- Mọi Controller phải kế thừa từ [`BaseApiController`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/src/ParkingService.Api/Controllers/BaseApiController.cs).
- Sử dụng các method tiện ích:
  - `return OkResponse(data);` cho HTTP 200 OK
  - `return OkResponse();` cho HTTP 200 OK không cần data
  - `return CreatedResponse(data);` cho HTTP 201 Created
  - `return ErrorResponse(StatusCodes.Status400BadRequest, "CODE", "Message", details);`

## 4. Xử Lý Exception Toàn Cục
- [`ExceptionHandlingMiddleware`](file:///D:/Projects/parking-manager/parking-service/core-parking-service/src/ParkingService.Api/Middleware/ExceptionHandlingMiddleware.cs) tự động bắt các exceptions:
  - `ValidationException` -> HTTP 400 (`VALIDATION_ERROR`, details chứa dictionary các trường lỗi)
  - `NotFoundException` -> HTTP 404 (`NOT_FOUND`)
  - `DomainException` -> HTTP 400 (`DOMAIN_ERROR`)
  - `UnauthorizedAccessException` -> HTTP 401 (`UNAUTHORIZED`)
  - Exception khác -> HTTP 500 (`INTERNAL_ERROR`, ẩn stack trace)
