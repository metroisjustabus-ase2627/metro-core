using FluentValidation.TestHelper;
using Metro.Application.Auth.Validators;
using Metro.Application.Users.Validators;
using Metro.Domain.Auth;
using Metro.Domain.Users;
using Shouldly;

namespace Metro.Application.Tests.Validators;

public class CommandValidatorTests
{
    [Fact]
    [Trait("Requirement", "FR-ACC-01")]
    [Trait("Requirement", "NFR-SEC-01")]
    public void CreateUserCommand_WhenPasswordInvalid_HasError()
    {
        var validator = new CreateUserCommandValidator();
        var result = validator.TestValidate(new Metro.Application.Users.Commands.CreateUserCommand
        {
            Name = "Ana",
            Email = "ana@example.com",
            Password = "123",
            Role = UserRoles.Passenger
        });

        result.ShouldHaveValidationErrorFor(command => command.Password)
            .WithErrorMessage(PasswordPolicy.RequirementsMessage);
    }

    [Theory]
    [Trait("Requirement", "FR-ACC-02")]
    [Trait("Requirement", "NFR-SEC-02")]
    [InlineData("admin")]
    [InlineData("user")]
    [InlineData("master")]
    [InlineData("client")]
    [InlineData("")]
    [InlineData("PASSENGER")]
    public void CreateUserCommand_WhenRoleInvalid_HasError(string role)
    {
        var validator = new CreateUserCommandValidator();
        var result = validator.TestValidate(new Metro.Application.Users.Commands.CreateUserCommand
        {
            Name = "Ana",
            Email = "ana@example.com",
            Password = "Secret123!",
            Role = role
        });

        result.ShouldHaveValidationErrorFor(command => command.Role)
            .WithErrorMessage(UserRoles.ValidationMessage);
    }

    [Theory]
    [Trait("Requirement", "FR-ACC-02")]
    [Trait("Requirement", "NFR-SEC-02")]
    [InlineData("admin")]
    [InlineData("user")]
    [InlineData("master")]
    [InlineData("client")]
    [InlineData("")]
    [InlineData("STAFF")]
    public void UpdateUserCommand_WhenRoleInvalid_HasError(string role)
    {
        var validator = new UpdateUserCommandValidator();
        var result = validator.TestValidate(new Metro.Application.Users.Commands.UpdateUserCommand
        {
            Id = Guid.NewGuid(),
            Name = "Ana",
            Email = "ana@example.com",
            Role = role
        });

        result.ShouldHaveValidationErrorFor(command => command.Role)
            .WithErrorMessage(UserRoles.ValidationMessage);
    }

    [Theory]
    [Trait("Requirement", "FR-ACC-01")]
    [Trait("Requirement", "FR-ACC-02")]
    [InlineData(UserRoles.Passenger)]
    [InlineData(UserRoles.Staff)]
    public void CreateUserCommand_WhenValidCanonicalRole_HasNoErrors(string role)
    {
        var validator = new CreateUserCommandValidator();
        var result = validator.TestValidate(new Metro.Application.Users.Commands.CreateUserCommand
        {
            Email = "ana@example.com",
            Password = "Secret123!",
            Role = role
        });

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    [Trait("Requirement", "FR-ACC-01")]
    public void CreateUserCommand_WithEmailAndPasswordOnly_DefaultsToPassengerAndIsValid()
    {
        var command = new Metro.Application.Users.Commands.CreateUserCommand
        {
            Email = "passenger@example.com",
            Password = "Secret123!"
        };

        command.Role.ShouldBe(UserRoles.Passenger);
        var validator = new CreateUserCommandValidator();
        validator.TestValidate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ResetPasswordCommand_WhenPasswordInvalid_HasError()
    {
        var validator = new ResetPasswordCommandValidator();
        var result = validator.TestValidate(new Metro.Application.Auth.Commands.ResetPasswordCommand
        {
            Email = "ana@example.com",
            Code = "123456",
            NewPassword = "123"
        });

        result.ShouldHaveValidationErrorFor(command => command.NewPassword)
            .WithErrorMessage(PasswordPolicy.RequirementsMessage);
    }

    [Fact]
    public void LoginCommand_WhenEmailMissing_HasError()
    {
        var validator = new LoginCommandValidator();
        var result = validator.TestValidate(new Metro.Application.Auth.Commands.LoginCommand
        {
            Email = "",
            Password = "Secret123!"
        });

        result.ShouldHaveValidationErrorFor(command => command.Email);
    }

    [Fact]
    public void RefreshTokenCommand_WhenMissing_HasError()
    {
        var validator = new RefreshTokenCommandValidator();
        var result = validator.TestValidate(new Metro.Application.Auth.Commands.RefreshTokenCommand
        {
            RefreshToken = " "
        });

        result.ShouldHaveValidationErrorFor(command => command.RefreshToken);
    }
}
