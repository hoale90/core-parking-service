using FluentValidation;

namespace ParkingService.Application.Features.Access.Commands.SwipeCard;

public class SwipeCardCommandValidator : AbstractValidator<SwipeCardCommand>
{
    public SwipeCardCommandValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Mã số thẻ không được để trống.")
            .MaximumLength(50).WithMessage("Mã số thẻ không được vượt quá 50 ký tự.");

        RuleFor(x => x.Direction)
            .IsInEnum().WithMessage("Hướng quẹt thẻ phải là In (1) hoặc Out (2).");

        When(x => x.CustomThresholdPercent.HasValue, () =>
        {
            RuleFor(x => x.CustomThresholdPercent!.Value)
                .InclusiveBetween(0.0, 100.0).WithMessage("Ngưỡng phần trăm so khớp biển số phải từ 0 đến 100.");
        });
    }
}
