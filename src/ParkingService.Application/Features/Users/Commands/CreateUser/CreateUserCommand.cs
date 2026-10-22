using MediatR;
using ParkingService.Application.Features.Users.DTOs;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Yêu cầu tạo mới thông tin người dùng / cư dân gửi xe.
/// </summary>
/// <param name="FullName">Họ và tên đầy đủ (bắt buộc, tối đa 200 ký tự)</param>
/// <param name="PlateNumber">Số xe / Biển số xe đăng ký (ví dụ: "59-X1 123.45", "38B1-28228")</param>
/// <param name="Phone">Số điện thoại liên hệ</param>
/// <param name="Email">Địa chỉ email</param>
/// <param name="Room">Số phòng / căn hộ (ví dụ: "A16", "B22")</param>
/// <param name="DateOfBirth">Ngày sinh (UTC)</param>
/// <param name="Gender">Giới tính</param>
/// <param name="Status">Trạng thái người dùng (1 = Active, 2 = Disabled)</param>
/// <param name="CardId">ID thẻ muốn liên kết ngay với người dùng (tùy chọn)</param>
public record CreateUserCommand(
    string FullName,
    string? PlateNumber = null,
    string? Phone = null,
    string? Email = null,
    string? Room = null,
    DateTime? DateOfBirth = null,
    string? Gender = null,
    UserStatus Status = UserStatus.Active,
    Guid? CardId = null
) : IRequest<UserDto>;
