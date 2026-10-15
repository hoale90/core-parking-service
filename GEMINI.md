# Antigravity & Gemini Agent Instructions

Xem quy chuẩn toàn diện tại file [AGENTS.md](file:///D:/Projects/parking-manager/parking-service/core-parking-service/AGENTS.md) và tài liệu yêu cầu nghiệp vụ tại [parking_management_requirements.md](file:///D:/Projects/parking-manager/parking-service/core-parking-service/parking_management_requirements.md).

## Tóm Tắt Quy Chuẩn Bắt Buộc

1. **Clean Architecture**:
   - `Domain` (không phụ thuộc package ngoài) -> `Application` (CQRS MediatR, FluentValidation) -> `Infrastructure` (EF Core SQLite, Repositories) -> `Api` (Controllers kế thừa `BaseApiController`).
2. **Chuẩn Hóa Response**:
   - Luôn trả về `{ "success": bool, "data": ... | null, "error": { "code": string, "message": string, "details": any } | null }`.
3. **Viết Test Thực Tế, Không Ảo**:
   - Mọi test case phải kiểm tra logic nghiệp vụ thực, boundary values, exception paths.
   - Không viết test rỗng, test `Assert.True(true)`, hoặc mock không có behavior.
4. **Phạm Vi Giai Đoạn 1**:
   - Chỉ triển khai `User` và `Card` API theo Phần 5 của `parking_management_requirements.md`.
5. **Database**:
   - SQLite, khóa chính UUID (string/Guid), thời gian UTC ISO 8601, hỗ trợ soft-delete.
