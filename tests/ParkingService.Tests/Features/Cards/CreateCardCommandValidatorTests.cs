using FluentValidation.TestHelper;
using ParkingService.Application.Features.Cards.Commands.CreateCard;
using ParkingService.Domain.Enums;
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

    [Fact]
    public void Validate_WhenVehiclePlateIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCardCommand(
            CardNumber: "0012345678",
            VehiclePlate: "59-X1@#$");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VehiclePlate)
            .WithErrorMessage("Biển số xe chứa ký tự không hợp lệ.");
    }

    [Fact]
    public void Validate_WhenPhoneIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCardCommand(
            CardNumber: "0012345678",
            Phone: "090abc123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("Số điện thoại chứa ký tự không hợp lệ.");
    }

    [Fact]
    public void Validate_WhenRoomExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCardCommand(
            CardNumber: "0012345678",
            Room: new string('R', 51));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Room)
            .WithErrorMessage("Số phòng không được vượt quá 50 ký tự.");
    }

    [Fact]
    public void Validate_WhenInfoExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCardCommand(
            CardNumber: "0012345678",
            Info: new string('I', 251));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Info)
            .WithErrorMessage("Thông tin bổ sung không được vượt quá 250 ký tự.");
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

    [Fact]
    public void Validate_WhenAllDataSqlFieldsAreProvided_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new CreateCardCommand(
            CardNumber: "2848255313",
            Type: CardType.Registered,
            VehiclePlate: "94L1-05580",
            SkipValidateVehiclePlate: false,
            Info: "DUONG THI PHUNG",
            Phone: "0912345678",
            Room: "B22",
            Status: CardStatus.Active);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
