using FluentValidation.TestHelper;
using ParkingService.Application.Features.Cards.Commands.CreateCard;
using Xunit;

namespace ParkingService.Tests.Features.Cards;

public class CreateCardCommandValidatorTests
{
    private readonly CreateCardCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WhenCardNumberIsEmpty_ShouldHaveValidationError(string? cardNumber)
    {
        // Arrange
        var command = new CreateCardCommand(CardNumber: cardNumber!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("Mã số thẻ không được để trống.");
    }

    [Theory]
    [InlineData("CARD#123")]
    [InlineData("CARD 123")]
    [InlineData("CARD@999")]
    public void Validate_WhenCardNumberHasSpecialCharacters_ShouldHaveValidationError(string invalidCardNumber)
    {
        // Arrange
        var command = new CreateCardCommand(CardNumber: invalidCardNumber);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("Mã số thẻ chỉ được chứa chữ cái, số, dấu gạch dưới và gạch nối.");
    }

    [Theory]
    [InlineData("0012345678")]
    [InlineData("CARD-001")]
    [InlineData("RFID_A1_99")]
    public void Validate_WhenCardNumberIsValid_ShouldNotHaveValidationErrors(string validCardNumber)
    {
        // Arrange
        var command = new CreateCardCommand(CardNumber: validCardNumber);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
