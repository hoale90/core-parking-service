using FluentAssertions;
using FluentValidation.TestHelper;
using ParkingService.Application.Features.Users.Commands.CreateUser;
using Xunit;

namespace ParkingService.Tests.Features.Users;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WhenFullNameIsEmpty_ShouldHaveValidationError(string? fullName)
    {
        // Arrange
        var command = new CreateUserCommand(FullName: fullName!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("Họ và tên không được để trống.");
    }

    [Fact]
    public void Validate_WhenFullNameExceeds200Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('A', 201);
        var command = new CreateUserCommand(FullName: longName);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("Họ và tên không được vượt quá 200 ký tự.");
    }

    [Theory]
    [InlineData("user name with spaces")]
    [InlineData("user@admin")]
    [InlineData("user#name")]
    public void Validate_WhenUsernameContainsInvalidCharacters_ShouldHaveValidationError(string invalidUsername)
    {
        // Arrange
        var command = new CreateUserCommand(
            FullName: "Nguyễn Văn A",
            Username: invalidUsername);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Tên tài khoản chỉ được chứa chữ cái, số, dấu chấm, gạch dưới và gạch nối.");
    }

    [Fact]
    public void Validate_WhenRoleIdIsZeroOrNegative_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateUserCommand(
            FullName: "Nguyễn Văn A",
            RoleId: 0);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleId)
            .WithErrorMessage("RoleId phải lớn hơn 0.");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-at-sign.com")]
    [InlineData("@nodomain.com")]
    public void Validate_WhenEmailIsInvalidFormat_ShouldHaveValidationError(string invalidEmail)
    {
        // Arrange
        var command = new CreateUserCommand(
            FullName: "Nguyễn Văn A",
            Email: invalidEmail);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Địa chỉ email không đúng định dạng.");
    }

    [Theory]
    [InlineData("0912abc345")]
    [InlineData("++84#12345")]
    public void Validate_WhenPhoneContainsInvalidCharacters_ShouldHaveValidationError(string invalidPhone)
    {
        // Arrange
        var command = new CreateUserCommand(
            FullName: "Trần Thị B",
            Phone: invalidPhone);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("Số điện thoại chứa ký tự không hợp lệ.");
    }

    [Fact]
    public void Validate_WhenDateOfBirthIsInFuture_ShouldHaveValidationError()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var command = new CreateUserCommand(
            FullName: "Lê Văn C",
            DateOfBirth: futureDate);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Ngày sinh phải nhỏ hơn ngày hiện tại.");
    }

    [Fact]
    public void Validate_WhenPlateNumberExceeds20Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longPlate = new string('A', 21);
        var command = new CreateUserCommand(
            FullName: "Nguyễn Văn An",
            PlateNumber: longPlate);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlateNumber)
            .WithErrorMessage("Số xe không được vượt quá 20 ký tự.");
    }

    [Theory]
    [InlineData("59-X1@123")]
    [InlineData("29A#88888")]
    public void Validate_WhenPlateNumberContainsInvalidCharacters_ShouldHaveValidationError(string invalidPlate)
    {
        // Arrange
        var command = new CreateUserCommand(
            FullName: "Nguyễn Văn An",
            PlateNumber: invalidPlate);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlateNumber)
            .WithErrorMessage("Số xe chứa ký tự không hợp lệ.");
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new CreateUserCommand(
            FullName: "Nguyễn Văn Nam",
            Username: "nam.nguyen",
            RoleId: 3,
            Role: "admin",
            Room: "A102",
            DateOfBirth: new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender: "Nam",
            Phone: "0912345678",
            Email: "nam.nguyen@example.com",
            PlateNumber: "59-X1 123.45");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
