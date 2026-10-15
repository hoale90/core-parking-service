using MediatR;
using ParkingService.Application.Features.Users.DTOs;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Yêu cầu tạo mới người dùng trong hệ thống bãi xe.
/// </summary>
/// <param name="FullName">Họ và tên đầy đủ (bắt buộc, tối đa 200 ký tự)</param>
/// <param name="DateOfBirth">Ngày sinh (UTC, phải nhỏ hơn ngày hiện tại)</param>
/// <param name="Gender">Giới tính (ví dụ: "Nam", "Nữ", "Khác")</param>
/// <param name="Phone">Số điện thoại liên hệ hợp lệ</param>
/// <param name="Email">Địa chỉ email liên hệ</param>
/// <param name="PlateNumber">Số xe / Biển số xe đăng ký (ví dụ: "59-X1 123.45")</param>
/// <param name="Status">Trạng thái người dùng (1 = Active, 2 = Disabled)</param>
public record CreateUserCommand(
    string FullName,
    DateTime? DateOfBirth = null,
    string? Gender = null,
    string? Phone = null,
    string? Email = null,
    string? PlateNumber = null,
    UserStatus Status = UserStatus.Active
) : IRequest<UserDto>;
