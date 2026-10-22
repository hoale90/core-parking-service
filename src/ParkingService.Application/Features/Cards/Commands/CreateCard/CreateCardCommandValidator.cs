using FluentValidation;

namespace ParkingService.Application.Features.Cards.Commands.CreateCard;

public class CreateCardCommandValidator : AbstractValidator<CreateCardCommand>
{
    public CreateCardCommandValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Mã số thẻ không được để trống.")
            .MaximumLength(50).WithMessage("Mã số thẻ không được vượt quá 50 ký tự.")
            .Matches(@"^[a-zA-Z0-9_\-]+$").WithMessage("Mã số thẻ chỉ được chứa chữ cái, số, dấu gạch dưới và gạch nối.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Loại thẻ không hợp lệ (1 = Registered, 2 = Temporary, 3 = Unknown).");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Trạng thái thẻ không hợp lệ (1 = Active, 2 = Blocked, 3 = Lost).");

        RuleFor(x => x.VehiclePlate)
            .MaximumLength(20).WithMessage("Biển số xe không được vượt quá 20 ký tự.")
            .Matches(@"^[a-zA-Z0-9.\-\s]+$").WithMessage("Biển số xe chứa ký tự không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.VehiclePlate));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự.")
            .Matches(@"^[0-9+\-\s()]*$").WithMessage("Số điện thoại chứa ký tự không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Room)
            .MaximumLength(50).WithMessage("Số phòng không được vượt quá 50 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Room));

        RuleFor(x => x.Info)
            .MaximumLength(250).WithMessage("Thông tin bổ sung không được vượt quá 250 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Info));
    }
}
