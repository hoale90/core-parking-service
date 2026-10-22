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
            .IsInEnum().WithMessage("Phân loại thẻ không hợp lệ (1 = Registered, 2 = Temporary, 3 = Unknown).");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Trạng thái thẻ không hợp lệ (1 = Active, 2 = Blocked, 3 = Lost).");
    }
}
