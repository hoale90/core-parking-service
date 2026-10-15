using FluentValidation;

namespace ParkingService.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ và tên không được để trống.")
            .MaximumLength(200).WithMessage("Họ và tên không được vượt quá 200 ký tự.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Địa chỉ email không đúng định dạng.")
            .MaximumLength(150).WithMessage("Email không được vượt quá 150 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự.")
            .Matches(@"^[0-9+\-\s()]*$").WithMessage("Số điện thoại chứa ký tự không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Gender)
            .MaximumLength(20).WithMessage("Giới tính không được vượt quá 20 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Gender));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow).WithMessage("Ngày sinh phải nhỏ hơn ngày hiện tại.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.PlateNumber)
            .MaximumLength(20).WithMessage("Số xe không được vượt quá 20 ký tự.")
            .Matches(@"^[a-zA-Z0-9.\-\s]+$").WithMessage("Số xe chứa ký tự không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.PlateNumber));
    }
}
