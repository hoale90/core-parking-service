using MediatR;
using ParkingService.Application.Features.Users.DTOs;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Yêu cầu tạo mới người dùng trong hệ thống bãi xe.
/// </summary>
/// <param name="FullName">Họ và tên đầy đủ (bắt buộc, tối đa 200 ký tự)</param>
/// <param name="Username">Tên tài khoản người dùng/nhân viên (tối đa 100 ký tự)</param>
/// <param name="RoleId">ID vai trò (1: Technical Admin, 3: Admin, 4: Staff,...)</param>
/// <param name="Role">Tên vai trò (ví dụ: "admin", "staff", "resident")</param>
/// <param name="Room">Số phòng / căn hộ cư dân (ví dụ: "B22", "A16")</param>
/// <param name="DateOfBirth">Ngày sinh (UTC, phải nhỏ hơn ngày hiện tại)</param>
/// <param name="Gender">Giới tính (ví dụ: "Nam", "Nữ", "Khác")</param>
/// <param name="Phone">Số điện thoại liên hệ hợp lệ</param>
/// <param name="Email">Địa chỉ email liên hệ</param>
/// <param name="PlateNumber">Số xe / Biển số xe đăng ký (ví dụ: "59-X1 123.45")</param>
/// <param name="Status">Trạng thái người dùng (1 = Active, 2 = Disabled)</param>
public record CreateUserCommand(
    string FullName,
    string? Username = null,
    int? RoleId = null,
    string? Role = null,
    string? Room = null,
    DateTime? DateOfBirth = null,
    string? Gender = null,
    string? Phone = null,
    string? Email = null,
    string? PlateNumber = null,
    UserStatus Status = UserStatus.Active
) : IRequest<UserDto>;
